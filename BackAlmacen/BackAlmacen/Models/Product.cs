namespace BackAlmacen.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("Product")]
    public partial class Product
    {
        public int id { get; set; }

        [StringLength(30)]
        public string product_name { get; set; }

        [StringLength(10)]
        public string type { get; set; }

        public int? stock { get; set; }

        public double? price { get; set; }

        [StringLength(200)]
        public string description { get; set; }
    }
}
