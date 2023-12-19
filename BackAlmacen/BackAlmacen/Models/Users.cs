namespace BackAlmacen.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    public partial class Users
    {
        public int id { get; set; }

        [StringLength(10)]
        public string username { get; set; }

        [StringLength(100)]
        public string password { get; set; }

        [StringLength(10)]
        public string type { get; set; }

        public double? wallet { get; set; }
    }
}
