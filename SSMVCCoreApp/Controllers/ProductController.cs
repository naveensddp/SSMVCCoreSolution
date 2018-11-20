using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using SSMVCCoreApp.Infrastructure.Abstract;
using SSMVCCoreApp.Infrastructure.Concrete;
using SSMVCCoreApp.Infrastructure.Entities;

namespace SSMVCCoreApp.Controllers
{
  public class ProductController : Controller
  {
    private readonly IOptions<StorageUtility> _storageUtility;
    private readonly IProductRepository _productRepository;
    private readonly IPhotoService _photoService;

    public ProductController(IOptions<StorageUtility> storageUtility, IProductRepository productRepository, IPhotoService photoService)
    {
      _storageUtility = storageUtility;
      _productRepository = productRepository;
      _photoService = photoService;
    }
    public async Task<IActionResult> Index()
    {
      List<Product> result = await _productRepository.GetAllProductsAsync();
      return View(result);
    }

    public ActionResult Create()
    {
      return View();
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<ActionResult> Create([Bind(include: "ProductName, Description, Price, Category, PhotoUrl")] Product product, IFormFile photo)
    {
      if (ModelState.IsValid)
      {
        product.PhotoUrl = await _photoService.UploadPhotoAsync(product.Category, photo);
        await _productRepository.CreateAsync(product);
        TempData["newproduct"] = $"New Product: {product.ProductName} with Id {product.ProductId}, added successfully";
        return RedirectToAction("Index");
      }
      return View(product);
    }

    public ActionResult Success()
    {
      return View();
    }

    public async Task<ActionResult> GetByCategory(string category)
    {
      ViewBag.category = category;
      var result = await _productRepository.FindProductsByCategoryAsync(category);
      return View(result);
    }

    public async Task<ActionResult> Edit(int productId)
    {
      var result = await _productRepository.FindProductByIDAsync(productId);
      return View(result);
    }
    [HttpPost, ValidateAntiForgeryToken]
    public async Task<ActionResult> Edit(Product product, IFormFile photo)
    {
      if (photo == null) { }
      else
      {
        if (await _photoService.DeletePhotoAsync(product.Category, product.PhotoUrl))
        {
          product.PhotoUrl = await _photoService.UploadPhotoAsync(product.Category, photo);
        }
      }
      await _productRepository.UpdateAsync(product);
      return RedirectToAction("Index");
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<ActionResult> Delete(int productId)
    {
      var result = await _productRepository.FindProductByIDAsync(productId);
      if (await _photoService.DeletePhotoAsync(result.Category, result.PhotoUrl))
      {
        await _productRepository.DeleteAsync(productId);
      }
      return RedirectToAction("Index");
    }

    public IActionResult About()
    {
      var result = _storageUtility.Value;
      ViewBag.storageData = result;
      return View();
    }
    public IActionResult Contact()
    {
      return View();
    }
  }
}