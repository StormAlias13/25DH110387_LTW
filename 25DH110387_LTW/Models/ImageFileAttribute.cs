using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Linq;
using System.Web;

namespace _25DH110387_LTW.Models
{
    public class ImageFileAttribute : ValidationAttribute
    {
        private readonly string[] _allowed = { ".png", ".jpg", ".jpeg", ".gif", ".webp" };

        public override bool IsValid(object value)
        {
            // Không chọn file thì bỏ qua (Edit không bắt buộc chọn lại ảnh)
            var file = value as HttpPostedFileBase;
            if (file == null || file.ContentLength == 0) return true;

            string ext = Path.GetExtension(file.FileName).ToLower();
            return _allowed.Contains(ext);
        }
    }
}