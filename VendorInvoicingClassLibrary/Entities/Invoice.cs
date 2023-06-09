﻿using System.ComponentModel.DataAnnotations.Schema;

namespace VendorInvoicingClassLibrary.Entities
{
    public class Invoice
    {
        public int InvoiceId { get; set; }

        public DateTime? InvoiceDate { get; set; }

        public DateTime? InvoiceDueDate
        {
            get
            {
                return InvoiceDate?.AddDays(Convert.ToDouble(PaymentTerm?.DueDays));
            }
        }

        public double? PaymentTotal { 
            get
            {
                return InvoiceLineItems?.Sum(i => i.Amount);
            }
        }

        public DateTime? PaymentDate { get; set; }

        // FK:
        public int PaymentTermsId { get; set; }

        // Nav to associated term:
        public PaymentTerms? PaymentTerm { get; set; }
        // Nav to All terms
        [NotMapped]
        public List<PaymentTerms>? PaymentTerms { get; set; }

        // FK:
        public int VendorId { get; set; }

        // Nav to vendor:
        public Vendor? Vendor { get; set; }

        // Nav to line items:
        public ICollection<InvoiceLineItem>? InvoiceLineItems { get; set; }

        public double? AmountPaid { get; set; } = 0.0;
    }
}
