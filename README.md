# BuscaTuHobby

**BuscaTuHobby** es una aplicación que te permite buscar y comparar precios de Gunplas y maquetas de distintas tiendas en línea. Esta aplicación utiliza **.NET** para el backend y **Angular** para el frontend, y sigue la estructura propuesta por **Jason Taylor** en su Clean Architecture (https://github.com/jasontaylordev/CleanArchitecture).

## Funcionalidades Principales (En Proceso)

- **Búsqueda de Productos:** Encuentra tus Gunplas y maquetas favoritas utilizando palabras clave y filtros. :construction_worker:
- **Comparación de Precios:** Compara los precios de un producto en diferentes tiendas en línea. :soon:
- **Detalles del Producto:** Obtén información detallada sobre cada producto, incluyendo descripción, reseñas y especificaciones. :construction_worker:
- **Gestión de Favoritos:** Guarda tus productos favoritos para acceder a ellos fácilmente más tarde. :soon:
- **Autenticación y Perfiles:** Crea una cuenta, inicia sesión y personaliza tu perfil. :construction_worker:
- **Comentarios y Valoraciones:** Comparte tus opiniones sobre los productos y lee las de otros usuarios. :soon:

## Configuración del Proyecto

Asegúrate de tener instaladas las siguientes herramientas antes de comenzar:

- .NET Core 10: https://dotnet.microsoft.com/download
- Node.js y npm: https://nodejs.org/
- Angular CLI: https://angular.io/cli

### Configuración del Proyecto 

1. Clona este repositorio: `git clone https://github.com/dcaceresc/BuscaTuHobby.git` 
2. Navega al directorio del proyecto y abre una terminal.
3. Ejecuta `dotnet restore` para restaurar las dependencias del proyecto.
4. Diriguete al directorio `/src/Web/WebUI`
5. Ejecuta `npm install` para instalar los paquetes de node
6. Regresa al directorio `/src/Web` y ejecuta `dotnet run`


## Vista previa de productos de Irion

Desde **Administración → Vista previa de Irion** (`/maintainer/irion-preview`), pega la URL de un producto de Irion y selecciona **Visualizar producto**. Disponible para `Administrator` y `SuperAdmin`.

La pantalla muestra nombre, marca, imágenes, etiquetas, disponibilidad y precios por variante. La descripción original se muestra como texto, sin ejecutar HTML. Los datos ausentes se indican explícitamente y los errores permiten volver a consultar sin perder la URL.

La consulta extrae **un producto por URL**, sin crear productos ni modificar la base de datos. El navegador muestra las imágenes desde sus direcciones públicas; el backend no las descarga ni almacena. No incluye rastreo del catálogo ni importación.

Con la API en ejecución y un token de `Administrator` o `SuperAdmin`:

```bash
curl -X POST 'https://localhost:7091/api/maintainer/products/preview/irion' \
  -H "Authorization: Bearer $TOKEN" \
  -H 'Content-Type: application/json' \
  --data '{"url":"https://irion.cl/products/transformers-classic-class-28-optimus-prime-energon-universe"}'
```

La respuesta usa `ApiResponse`: comprueba `success` antes de leer `data`. Incluye nombre, marca, etiquetas, descripción HTML **no sanitizada**, enlaces de imágenes, disponibilidad y variantes con SKU, código de barras y precios en CLP. No insertes `descriptionHtml` como HTML confiable. Las fechas de lanzamiento y la inclusión de base no se deducen ni se inventan.

- Admite `https://irion.cl/products/{handle}` y `https://irion.cl/collections/{collection}/products/{handle}`, con letras ASCII y números separados por guiones en ambos identificadores. Los parámetros de seguimiento y fragmentos se eliminan. Ambas rutas hacen una sola consulta a `https://irion.cl/products/{handle}.js`; la URL de origen devuelta no incluye `.js`.
- Obtiene el JSON público de Shopify; no sigue redirecciones. Límite de respuesta: 1 MiB; tiempo máximo de consulta: 15 segundos, además de la cancelación del cliente.
- Convierte los precios de Shopify dividiendo por 100. Si no hay precio original, `originalPrice` y `discountPercentage` son `null`. La disponibilidad no es una cantidad de stock ni una garantía de compra.
- Una URL no admitida devuelve `success: false`, `data: null` y `Introduce una URL válida de Irion: https://irion.cl/products/nombre-del-producto o https://irion.cl/collections/coleccion/products/nombre-del-producto`, sin consultar la tienda.
- Los demás errores de extracción devuelven `success: false` y el mensaje `No se pudo obtener la vista previa del producto de Irion.` dentro de una respuesta HTTP 200, siguiendo el patrón existente. La autenticación y autorización conservan sus respuestas HTTP habituales.
- Si Irion responde HTTP 429, devuelve `success: false`, `data: null` y `Irion está limitando las consultas (HTTP 429). Inténtalo más tarde.` (también dentro de HTTP 200). La pantalla muestra ese mensaje sin perder la URL. Si `Retry-After` contiene segundos válidos (hasta 2147483647), indica la espera mínima; si contiene una fecha futura válida, indica cuándo volver a consultar en UTC. Si falta, está mal formado, excede ese límite o contiene una fecha pasada, muestra el mensaje anterior. No se realizan reintentos automáticos ni consultas alternativas: espera lo indicado y pulsa de nuevo **Visualizar producto**. La espera no garantiza que la tienda haya levantado el límite. Admitir ambas rutas no elimina los límites impuestos por la tienda.
- No realiza consultas periódicas. Revisa las condiciones de reutilización de la tienda antes de publicar su catálogo y respeta sus límites de acceso.

Pruebas del frontend (desde `src/Web/WebUI/`):

```bash
pnpm exec ng test --watch=false --include='**/irion-preview.component.spec.ts'
pnpm test:run
pnpm build
```

Pruebas deterministas del backend, sin red ni base de datos (ejecutable de comprobación, no suite de `dotnet test`):

```bash
dotnet run --project tests/IrionProductExtractor.Checks/IrionProductExtractor.Checks.csproj
```

## Contribución

¡Estamos abiertos a contribuciones! Si deseas colaborar en el proyecto, sigue estos pasos:

1. Realiza un fork del repositorio.
2. Crea una rama para tu contribución: `git checkout -b feature/nueva-funcionalidad`.
3. Desarrolla y realiza tus cambios.
4. Envía un pull request a la rama principal del proyecto.

## Problemas

Si tiene problemas, hágamelo saber planteando un [nuevo problema](https://github.com/dcaceresc/BuscaTuHobby/issues).

## Licencia

Este proyecto se distribuye bajo la licencia [MIT](LICENSE).



¡Gracias por utilizar BuscaTuHobby!

