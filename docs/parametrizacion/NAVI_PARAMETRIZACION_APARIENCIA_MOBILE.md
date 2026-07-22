# NAVI Herramientas y Activos
## Parametrización de apariencia Mobile PWA

Fecha: 2026-07-13

Ubicación:

- Configuración móvil.
- Ruta: `/mobile/settings`.

Persistencia:

- Clave: `navi-mobile-ui-preferences-v1`.
- Medio: `localStorage`.
- Alcance: dispositivo y navegador actual.
- API: no aplica.
- Base de datos: no aplica.

Tamaños:

- `compact`: compacto.
- `normal`: normal, base móvil de 13 px.
- `large`: grande.
- `extra-large`: muy grande.

Temas:

- `normal`: normal corporativo.
- `cold`: colores fríos.
- `warm`: colores cálidos.
- `dark`: colores oscuros.

Acciones afirmativas:

- Guardar.
- Confirmar.
- Validar.
- Aceptar.
- Aprobar.
- Enviar.

Color:

- Verde primary.
- Texto blanco cuando el fondo sea sólido.

Acciones operacionales:

- Editar.
- Asignar.
- Gestionar.
- Crear.
- Solicitar.
- Reportar.

Color:

- Morado primary.
- Texto blanco cuando el fondo sea sólido.

Acciones destructivas:

- Rechazar.
- Denegar.
- Eliminar.
- Anular.
- Dar de baja.

Color:

- Borde y texto rojo por defecto.
- Fondo rojo únicamente cuando sea una confirmación crítica.

Acciones de regreso:

- Regresar.
- Devolver.
- Reabrir.
- Retomar.

Color:

- Advertencia naranja o amarilla.

Acciones secundarias:

- Volver.
- Atrás.
- Limpiar.
- Cancelar edición.

Acciones de expansión:

- Expandir.
- Mostrar.
- Ocultar.
- Contraer.

Regla:

- El contenido puede ocultarse.
- El encabezado del panel siempre debe permanecer visible.

Condiciones móviles:

- El encabezado estándar nunca se elimina.
- Actualizar continúa perteneciendo al encabezado.
- No se duplica menú, avatar ni notificaciones.
- No debe existir desplazamiento horizontal de página.
- Debe funcionar en 360 px y 390 px.
- Web y Mobile conservan preferencias independientes.
- No se modifica API, permisos, rutas ni lógica funcional.
