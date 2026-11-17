using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace _25DH110387_LTW.Models
{
    [MetadataType(typeof(UserMetadata))]
    public partial class User
    {

    }
    [MetadataType(typeof(ProductMetadata))]
    public partial class Product
    {
        [ImageFile(ErrorMessage = "Chỉ chấp nhận ảnh PNG, JPG, JPEG, GIF, WEBP")]
        [NotMapped]
        public HttpPostedFileBase UploadImg { get; set; }
    }
    [MetadataType(typeof(OrderMetadata))]
    public partial class Order
    {
        
       
       

    }
    [MetadataType(typeof(OrderDetailMetadata))]
    public partial class OrderDetail
    {
        
    }
}