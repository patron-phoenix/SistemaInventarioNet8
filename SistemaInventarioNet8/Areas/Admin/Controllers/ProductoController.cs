using Microsoft.AspNetCore.Mvc;
using Microsoft.Win32;
using SistemaInventarioNet8.AccesoDatos.Repositorio.IRepositorio;
using SistemaInventarioNet8.Modelos;
using SistemaInventarioNet8.Modelos.ViewModels;
using SistemaInventarioNet8.Utilidades;

namespace SistemaInventarioNet8.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class ProductoController : Controller
    {

        private readonly IUnidadTrabajo _unidadTrabajo;
        private readonly IWebHostEnvironment _webHostEnvironment;

        public ProductoController(IUnidadTrabajo unidadTrabajo, IWebHostEnvironment webHostEnvironment)
        {
            _unidadTrabajo=unidadTrabajo;
            _webHostEnvironment=webHostEnvironment;
        }


        public IActionResult Index()
        {
            return View();
        }

        public async Task<IActionResult> Upsert(int? id)
        {
            ProductoVM productoVM = new ProductoVM()
            {
                Producto = new Producto(),
                CategoriaLista = _unidadTrabajo.Producto.ObtenerTodosDropdownLista("Categoria"),
                MarcaLista = _unidadTrabajo.Producto.ObtenerTodosDropdownLista("Marca")
            };

            if(id == null)
            {
                //nuevo registro
                productoVM.Producto.Estado = true;
                return View(productoVM);
            }
            else
            {
                //actualizamos registro
                productoVM.Producto=await _unidadTrabajo.Producto.Obtener(id.GetValueOrDefault());
                if (productoVM.Producto == null)
                {
                    return NotFound();
                }
                return View(productoVM);
            }
        
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Upsert(ProductoVM productoVM)
        {
            if (ModelState.IsValid)
            {
                var files=HttpContext.Request.Form.Files;
                string webRootPath = _webHostEnvironment.WebRootPath;

                if(productoVM.Producto.Id == 0)
                {
                    //crear nuevo registro
                    string upload = webRootPath + DS.ImagenRuta;
                    string fileName= Guid.NewGuid().ToString();
                    string extension = Path.GetExtension(files[0].FileName);

                    using(var fileStream= new FileStream(Path.Combine(upload,fileName+extension),FileMode.Create))
                    {
                        files[0].CopyTo(fileStream);
                    }

                   
                    productoVM.Producto.ImagenUrl = fileName + extension;
                    await _unidadTrabajo.Producto.Agregar(productoVM.Producto);
                }
                else
                {
                    //actualizar registro
                    var objProducto = await _unidadTrabajo.Producto.ObtenerPrimero(p=>p.Id == productoVM.Producto.Id, isTracking:false);
                    if(files.Count > 0) //si se carga una nueva imagen
                    {
                        string upload = webRootPath + DS.ImagenRuta;
                        string fileName = Guid.NewGuid().ToString();
                        string extension = Path.GetExtension(files[0].FileName);

                        //borrar la imagen anterior
                        var anteriorFile = Path.Combine(upload, objProducto.ImagenUrl);
                        if (System.IO.File.Exists(anteriorFile))
                        {
                            System.IO.File.Delete(anteriorFile);
                        }
                        using (var fileStream = new FileStream(Path.Combine(upload, fileName + extension), FileMode.Create))
                        {
                            files[0].CopyTo(fileStream);
                        }
                        productoVM.Producto.ImagenUrl = fileName + extension;
                        
                    }// sino se carga una nueva imagen se mantiene
                    else
                    {
                        productoVM.Producto.ImagenUrl = objProducto.ImagenUrl;
                    }

                    _unidadTrabajo.Producto.Actualizar(productoVM.Producto);
                }

                TempData[DS.Exitosa] = "Transaccion Exitosa";
                await _unidadTrabajo.Guardar();
                return View("Index");
            }// si no esvalido cargamos las listas

            productoVM.CategoriaLista = _unidadTrabajo.Producto.ObtenerTodosDropdownLista("Categoria");
            productoVM.MarcaLista = _unidadTrabajo.Producto.ObtenerTodosDropdownLista("Marca");
            return View(productoVM);
        }



        #region

        [HttpGet]
        public async Task<IActionResult> ObtenerTodos()
        {
            var todos = await _unidadTrabajo.Producto.ObtenerTodos(incluirPropiedades:"Categoria,Marca");
            return Json(new {data=todos});
        }

        [HttpPost]
        public async Task<IActionResult> Delete(int id)
        {
            var productoDB = await _unidadTrabajo.Producto.Obtener(id);
            if (productoDB == null)
            {
                return Json(new { success=false, message = "Error al borrar Producto" });
            }

            //eliminamos la imagen 
            string upload = _webHostEnvironment.WebRootPath + DS.ImagenRuta;
            var anteriorFile = Path.Combine(upload, productoDB.ImagenUrl);

            if (System.IO.File.Exists(anteriorFile))
            {
                System.IO.File.Delete(anteriorFile);
            }

            _unidadTrabajo.Producto.Remover(productoDB);
            await _unidadTrabajo.Guardar();
            return Json(new { success = true, message = "Producto Borrada exitosamente" });
        }


        [ActionName("ValidarSerie")]
        public async Task<IActionResult> ValidarSerie(string serie, int id = 0)
        {
            bool valor = false;
            var lista = await _unidadTrabajo.Producto.ObtenerTodos();

            if (id == 0)
            {
                valor = lista.Any(b => b.NumeroSerie.ToLower().Trim() == serie.ToLower().Trim());
            }
            else
            {
                valor = lista.Any(b => b.NumeroSerie.ToLower().Trim() == serie.ToLower().Trim() && b.Id != id);
            }

            if (valor)
            {
                return Json(new { data = true });
            }
            return Json(new { data = false });

        }


        #endregion

    }
}
