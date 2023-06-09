﻿using Microsoft.EntityFrameworkCore;
using VendorInvoicing.DataAccess;
using VendorInvoicingClassLibrary.Services;
using VendorInvoicingClassLibrary.Entities;

namespace VendorInvoicing.Services
{
    //Vendor Db service that implements all db functionality found in the IvendorIncvoicingService Interface
    public class DbVendorInvoicingService : IVendorInvoicingService
    {
        private VendorsContext _vendorsContext;
        public DbVendorInvoicingService(VendorsContext vendorInvoicingService)
        {
            _vendorsContext = vendorInvoicingService;
        }
        //Add Invoice to Invoice Table (vendorId must be present)
        public bool AddInvoice(Invoice invoice)
        {
            _vendorsContext.Invoices.Add(invoice);

            return _vendorsContext.SaveChanges() != 0 ? true : false;

        }
        //Add InvoiceLineItem to InvoiceLineItem Table (invoiceId must be present)
        public bool AddInvoiceLineItem(InvoiceLineItem invoiceLineItem)
        {
            _vendorsContext.InvoiceLineItems.Add(invoiceLineItem);
            return _vendorsContext.SaveChanges() != 0 ? true : false;
        }
        //Add Vendor to Vendor Table
        public bool AddVendor(Vendor vendor)
        {
            _vendorsContext.Vendors.Add(vendor);
            return _vendorsContext.SaveChanges() != 0 ? true : false;
        }
        //Delete all "IsDeleted" Vendors
        public bool DeleteAllisDeletedVendors()
        {
            var deletedVendors = _vendorsContext.Vendors.Where(v => v.IsDeleted == true)
                .Include(v => v.Invoices)
                    .ThenInclude(i => i.InvoiceLineItems)
                .ToList();

            if (deletedVendors.Count() > 0)
            {
                foreach (var vendor in deletedVendors)
                {
                    foreach (var invoice in vendor.Invoices)
                    {
                        foreach (var invoiceLineItem in invoice.InvoiceLineItems)
                        {
                            _vendorsContext.InvoiceLineItems.Remove(invoiceLineItem);
                        }
                        _vendorsContext.Invoices.Remove(invoice);
                    }
                    _vendorsContext.Vendors.Remove(vendor);
                }
                _vendorsContext.SaveChanges();
                return true;
            }
            return false;
        }
        //Delete Vendor From Vendor Table by vendorId (Changes IsDeleted)
        public bool DeleteVendorById(int id)
        {
            Vendor vendor = _vendorsContext.Vendors.Where(v => v.VendorId == id).FirstOrDefault();
            if (vendor != null)
            {
                vendor.IsDeleted = true;
                _vendorsContext.Vendors.Update(vendor);
                _vendorsContext.SaveChanges();
                return true;
            }
            return false;
        }
        //Retrieve all Invoices from Invoices table for selected vendor
        public ICollection<Invoice> GetAllInvoices(int vendorId)
        {
            return _vendorsContext.Invoices.Where(i => i.VendorId == vendorId).ToList();
        }
        //Retrieve all Vendors from vendors table
        public ICollection<Vendor> GetAllVendors()
        {
            ICollection<Vendor> vendors;
            vendors = _vendorsContext.Vendors
                .Include(v => v.Invoices)
                    .ThenInclude(i => i.InvoiceLineItems)
                .Include(v => v.Invoices)
                    .ThenInclude(i => i.PaymentTerm)
                .OrderBy(v => v.Name)
                .ToList();
            if (vendors != null)
            {
                foreach (Vendor v in vendors)
                {
                    foreach (Invoice invoice in v.Invoices)
                    {
                        invoice.PaymentTerms = _vendorsContext.PaymentTerms
                            .OrderBy(pt => pt.PaymentTermsId)
                            .ToList();
                    }
                }
            }
            
            return vendors;
        }
        //Get First Vendor that has "IsDeleted = True"
        public int? GetDeletedVendorId()
        {
            return _vendorsContext.Vendors?.Where(v => v.IsDeleted == true).FirstOrDefault()?.VendorId;
        }
        //Get specific Invoice from Invoice table
        public Invoice GetInvoiceById(int id)
        {
            return _vendorsContext.Invoices.Where(i => i.InvoiceId == id).FirstOrDefault();
        }

        public ICollection<PaymentTerms> GetPaymentTerms()
        {
            return _vendorsContext.PaymentTerms.ToList();
        }

        //Get specific Vendor from Vendor table
        public Vendor GetVendorById(int id)
        {
            Vendor vendor = _vendorsContext.Vendors
                .Where(v => v.VendorId == id)
                .Include(v => v.Invoices)
                    .ThenInclude(i => i.InvoiceLineItems)
                .Include(v => v.Invoices)
                    .ThenInclude(i => i.PaymentTerm)
                .OrderBy(v => v.Name)
                .FirstOrDefault();
            if (vendor != null)
            {
                foreach (Invoice invoice in vendor.Invoices)
                {
                    invoice.PaymentTerms = _vendorsContext.PaymentTerms
                        .OrderBy(pt => pt.PaymentTermsId)
                        .ToList();
                }
            }

            return vendor;
        }
        //Get Vendors from vendors Table in Range 
        public ICollection<Vendor> GetVendorsInRangeAZ(string startingLetter, string endingLetter)
        {
            return _vendorsContext.Vendors
                .OrderBy(v => v.Name)
                .Where(v => v.Name.CompareTo(startingLetter) >= 0 && v.Name.CompareTo(endingLetter) <= 0)
                .ToList();
        }
        //Undo "IsDeleted" Tag of Vendor
        public bool UndoDeleteVendorById(int id)
        {
            Vendor vendor = _vendorsContext.Vendors.Where(v => v.VendorId == id).FirstOrDefault();
            if (vendor != null)
            {
                vendor.IsDeleted = false;
                _vendorsContext.Vendors.Update(vendor);
                _vendorsContext.SaveChanges();
                return true;
            }
            return false;
        }
        //Update Vendor Information for a specific Vendor in the Vendors Table
        public bool UpdateVendor(Vendor vendor)
        {
            Vendor vendorFromDB = _vendorsContext.Vendors.Where(v => v.VendorId == vendor.VendorId).FirstOrDefault();
            if (vendorFromDB != null)
            {
                _vendorsContext.Vendors.Update(vendor);
                _vendorsContext.SaveChanges();
                return true;
            }
            return false;
        }
    }
}
