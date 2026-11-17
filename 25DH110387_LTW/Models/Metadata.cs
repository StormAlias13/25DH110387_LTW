using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.ComponentModel;

namespace _25DH110387_LTW.Models
{
    public class UserMetadata
    {
        [Required(ErrorMessage = "Username is required.")]
        [StringLength(30, MinimumLength = 5, ErrorMessage = "Username must be between 5 and 30 characters.")]
        [RegularExpression("^[a-zA-Z0-9]([._-](?![._-])|[a-zA-Z0-9]){3,18}[a-zA-Z0-9]$")]
        public string Username { get; set; }
        //[Required]
        //[DataType(DataType.Password)]
        //public string Password { get; set; }
        //[Required]
        //public string UserRole { get; set; }


        [Required(ErrorMessage = "Phải nhập mật khẩu")]
        [StringLength(50, MinimumLength = 5, ErrorMessage = "Mật khẩu từ 5 đến 50 ký tự")]
        [DataType(DataType.Password)]
        [Display(Name = "Mật khẩu")]
        public string Password { get; set; }
        public string UserRole { get; set; }
    }
    public class CustomerMetadata
    {
        [Display(Name = "Mã khách hàng")]
        public int CustomerID { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập họ tên")]
        [StringLength(100, ErrorMessage = "Họ tên tối đa 100 ký tự")]
        [Display(Name = "Họ và tên")]
        public string CustomerName { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập số điện thoại")]
        [RegularExpression(@"^0(3|5|7|8|9)[0-9]{8}$",
         ErrorMessage = "Số điện thoại phải gồm 10 chữ số, bắt đầu bằng 03, 05, 07, 08 hoặc 09")]
        [StringLength(10, MinimumLength = 10, ErrorMessage = "Số điện thoại phải đúng 10 chữ số")]
        [Display(Name = "Số điện thoại")]
        public string CustomerPhone { get; set; }

        [EmailAddress(ErrorMessage = "Email không hợp lệ")]
        [StringLength(100, ErrorMessage = "Email tối đa 100 ký tự")]
        [Display(Name = "Địa chỉ Email")]
        public string CustomerEmail { get; set; }

        
        [Display(Name = "Địa chỉ")]
        public string CustomerAddress { get; set; }

        
        [StringLength(255, ErrorMessage = "Tài khoản tối đa 255 ký tự")]
        [Display(Name = "Tài khoản")]
        public string Username { get; set; }
    }
    public class CategoryMetadata
    {
        [HiddenInput]
        public int CategoryID { get; set; }
        public string CategoryName { get; set; }
    }
    public class ProductMetadata
    {

        //public int ProductID { get; set; }
        //public int CategoryID { get; set; }
        //public string ProductName { get; set; }
        //public string ProductDescription { get; set; }
        //public decimal ProductPrice { get; set; }

        //public string ProductImage { get; set; }
        [Display(Name = "Mã sản phẩm")]
        public int ProductID { get; set; }

        public int CategoryID { get; set; }

        [StringLength(100)]
        [Required(ErrorMessage = "Phải nhập tên sản phẩm")]
        
        public string ProductName { get; set; }

        [Required(ErrorMessage = "Phải nhập mô tả")]
        
        public string ProductDescription { get; set; }

        [Required(ErrorMessage = "Phải nhập đơn giá")]
        [Range(0, 999999999, ErrorMessage = "Đơn giá phải là số không âm")]
        [Display(Name = "Đơn giá")]
        public decimal ProductPrice { get; set; }

        [Display(Name = "Đường dẫn ảnh sản phẩm")]
        [DefaultValue("~/Content/images/default_img.png")]
        public string ProductImage { get; set; }

    }
    public class OrderMetadata
    {
        public int OrderID { get; set; }
        public int CustomerID { get; set; }
        public decimal TotalAmount { get; set; }
        public string PaymentStatus { get; set; }
        public string ShippingAddress { get; set; }

    }
    public class OrderDetailMetadata 
    {
        public int ID { get; set; }
        public int ProductID { get; set; }
        public int OrderID { get; set; }
        public int Quantity { get; set; }
        public decimal UnitPrice { get; set; }

    }
    
}