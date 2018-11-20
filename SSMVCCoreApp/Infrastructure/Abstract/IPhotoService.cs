using System.Web;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace SSMVCCoreApp.Infrastructure.Abstract
{
  public interface IPhotoService
  {
    //Task<string> UploadPhotoAsync(string category, HttpPostedFileBase photoToUpload);
    Task<string> UploadPhotoAsync(string category, IFormFile photoToUpload);
    Task<bool> DeletePhotoAsync(string category, string photoUrl);
  }
}
