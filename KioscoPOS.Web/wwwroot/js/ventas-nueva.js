const { createApp } = Vue;

createApp({
    data() {
        return {
            termino: "",
            sugerencias: [],
            carrito: [],
            medioPago: 1, // 1 = Efectivo
            cargando: false,
            mensajeError: "",
            ventaOk: null,
            ultimaBusqueda: 0, // para ignorar respuestas viejas del autocomplete
            timer: null,
        };
    },
    computed: {
        // solo informativo: el servidor recalcula el total con los precios de la base
        total() {
            return this.carrito.reduce(
                (acc, i) => acc + i.cantidad * i.precioVenta,
                0,
            );
        },
    },
    methods: {
        dinero(n) {
            return Number(n).toLocaleString("es-AR", {
                minimumFractionDigits: 2,
                maximumFractionDigits: 2,
            });
        },

        // autocomplete por nombre/codigo: espera 250ms desde la ultima tecla
        buscarSugerencias() {
            clearTimeout(this.timer);
            const q = this.termino.trim();
            if (q.length < 2) {
                this.sugerencias = [];
                return;
            }
            this.timer = setTimeout(async () => {
                const numero = ++this.ultimaBusqueda;
                try {
                    const res = await fetch(
                        `/Busqueda/Productos?q=${encodeURIComponent(q)}`,
                    );
                    if (!res.ok) return;
                    const data = await res.json();
                    // si mientras tanto se hizo otra busqueda, esta respuesta ya no sirve
                    if (numero === this.ultimaBusqueda) this.sugerencias = data;
                } catch (e) {
                    /* el autocomplete no es critico, se ignora */
                }
            }, 250);
        },

        // el lector de barras escribe el codigo y manda Enter
        async enter() {
            const codigo = this.termino.trim();
            if (!codigo) return;
            this.mensajeError = "";
            this.ventaOk = null;

            try {
                const res = await fetch(
                    `/Busqueda/PorCodigo?codigo=${encodeURIComponent(codigo)}`,
                );
                if (res.ok) {
                    this.agregar(await res.json());
                    return;
                }
                // no era un codigo exacto: si el autocomplete tiene un unico resultado, se usa
                if (this.sugerencias.length === 1) {
                    this.agregar(this.sugerencias[0]);
                    return;
                }
                const data = await res.json().catch(() => ({}));
                this.mensajeError =
                    this.sugerencias.length > 1
                        ? "Elegí un producto de la lista"
                        : data.mensaje || "Producto no encontrado";
            } catch (e) {
                this.mensajeError = "Error al buscar el producto";
            }
        },

        agregar(p) {
            this.mensajeError = "";
            this.ventaOk = null;

            if (p.stockActual <= 0) {
                this.mensajeError = `"${p.nombre}" no tiene stock`;
                this.terminarEntrada();
                return;
            }

            const existente = this.carrito.find((i) => i.productoId === p.id);
            if (existente) {
                this.cambiarCantidad(existente, existente.cantidad + 1);
            } else {
                this.carrito.push({
                    productoId: p.id,
                    nombre: p.nombre,
                    precioVenta: p.precioVenta,
                    stockActual: p.stockActual,
                    cantidad: 1,
                });
            }
            this.terminarEntrada();
        },

        // deja el input listo para escanear el proximo producto
        terminarEntrada() {
            this.termino = "";
            this.sugerencias = [];
            this.ultimaBusqueda++; // descarta autocompletes en vuelo
            this.$refs.entrada.focus();
        },

        cambiarCantidad(item, valor) {
            let cantidad = Math.floor(Number(valor));
            if (!Number.isFinite(cantidad) || cantidad < 1) cantidad = 1;

            if (cantidad > item.stockActual) {
                this.mensajeError = `Solo hay ${item.stockActual} unidades de "${item.nombre}"`;
                cantidad = item.stockActual;
            }
            item.cantidad = cantidad;
        },

        quitar(item) {
            this.carrito = this.carrito.filter((i) => i !== item);
        },

        vaciar() {
            if (!confirm("¿Vaciar el carrito?")) return;
            this.carrito = [];
            this.mensajeError = "";
        },

        async confirmar() {
            this.mensajeError = "";
            this.ventaOk = null;
            if (this.carrito.length === 0) return;

            this.cargando = true;
            try {
                const res = await fetch("/Ventas/Confirmar", {
                    method: "POST",
                    headers: { "Content-Type": "application/json" },
                    // solo producto y cantidad: el precio lo pone el servidor
                    body: JSON.stringify({
                        medioPago: this.medioPago,
                        items: this.carrito.map((i) => ({
                            productoId: i.productoId,
                            cantidad: i.cantidad,
                        })),
                    }),
                });
                const data = await res.json().catch(() => ({}));
                if (!res.ok) {
                    this.mensajeError = data.mensaje || "No se pudo registrar la venta";
                    return;
                }

                this.ventaOk = data.id;
                this.carrito = [];
                this.medioPago = 1;
                this.$refs.entrada.focus();
            } catch (e) {
                this.mensajeError = e.message;
            } finally {
                this.cargando = false;
            }
        },
    },
}).mount("#app-venta");
