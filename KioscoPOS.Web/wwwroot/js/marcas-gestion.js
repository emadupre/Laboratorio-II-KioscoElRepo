const { createApp } = Vue;

createApp({
  data() {
    return {
      items: [],
      buscar: "",
      page: 1,
      pageSize: 10,
      totalPages: 1,
      totalItems: 0,
      cargando: false,
      form: { id: 0, nombre: "" },
      editando: false,
      mensajeError: "",
      mensajeOk: "",
    };
  },
  mounted() {
    this.cargar(1);
  },
  methods: {
    async cargar(page) {
      if (page < 1) return;
      this.cargando = true;
      this.limpiarMensajes();
      try {
        const url = `/Marcas/ListarJson?buscar=${encodeURIComponent(this.buscar)}&page=${page}&pageSize=${this.pageSize}`;
        const res = await fetch(url);
        if (!res.ok) throw new Error("Error al cargar la lista");
        const data = await res.json();
        this.items = data.items;
        this.page = data.page;
        this.totalPages = data.totalPages;
        this.totalItems = data.totalItems;
      } catch (e) {
        this.mensajeError = e.message;
      } finally {
        this.cargando = false;
      }
    },
    async guardar() {
      this.limpiarMensajes();
      const nombre = (this.form.nombre || "").trim();
      if (!nombre) {
        this.mensajeError = "El nombre es obligatorio";
        return;
      }

      this.cargando = true;
      try {
        const editando = this.editando;
        const url = editando
          ? `/Marcas/EditarJson/${this.form.id}`
          : `/Marcas/CrearJson`;
        const method = editando ? "PUT" : "POST";
        const res = await fetch(url, {
          method,
          headers: { "Content-Type": "application/json" },
          body: JSON.stringify({ id: this.form.id, nombre }),
        });
        const data = await res.json().catch(() => ({}));
        if (!res.ok) {
          this.mensajeError = data.mensaje || "Error al guardar";
          return;
        }

        this.mensajeOk = editando ? "Marca actualizada" : "Marca creada";
        this.cancelarEdicion();
        await this.cargar(editando ? this.page : 1);
      } catch (e) {
        this.mensajeError = e.message;
      } finally {
        this.cargando = false;
      }
    },
    editar(m) {
      this.form = { id: m.id, nombre: m.nombre };
      this.editando = true;
      this.limpiarMensajes();
    },
    cancelarEdicion() {
      this.form = { id: 0, nombre: "" };
      this.editando = false;
    },
    async eliminar(m) {
      if (!confirm(`¿Eliminar la marca "${m.nombre}"?`)) return;
      this.limpiarMensajes();
      this.cargando = true;
      try {
        const res = await fetch(`/Marcas/EliminarJson/${m.id}`, {
          method: "DELETE",
        });
        const data = await res.json().catch(() => ({}));
        if (!res.ok) {
          this.mensajeError = data.mensaje || "Error al eliminar";
          return;
        }
        this.mensajeOk = "Marca eliminada";
        await this.cargar(this.page);
      } catch (e) {
        this.mensajeError = e.message;
      } finally {
        this.cargando = false;
      }
    },
    limpiarMensajes() {
      this.mensajeError = "";
      this.mensajeOk = "";
    },
  },
}).mount("#app-marcas");