﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using iPhoneBE.Data.Entities;

namespace iPhoneBE.Data.Model
{
    public class Order
    {
        [Key]
        public int OrderID { get; set; } 

        public int UserID { get; set; }

        public int? ShipperID { get; set; }

        public DateTime OrderDate { get; set; } 

        [MaxLength(500)]
        public string Address { get; set; } 

        [MaxLength(500)]
        public string PaymentMethod { get; set; } 

        [ForeignKey("ShippingMethod")]
        public int ShippingMethodID { get; set; } 

        public double Total { get; set; } 

        [MaxLength(250)]
        public string OrderStatus { get; set; } 

        [ForeignKey("Voucher")]
        public int? VoucherID { get; set; } 

        public User User { get; set; }
        public User Shipper { get; set; }
        public ShippingMethod ShippingMethod { get; set; }
        public Voucher Voucher { get; set; }
        public ICollection<OrderDetail> OrderDetails { get; set; }
        public ICollection<PaypalTransaction> PaypalTransactions { get; set; }
    }
}
