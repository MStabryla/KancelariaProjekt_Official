using Microsoft.AspNetCore.Http;
using SWI2DB.Models.Authentication;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace SWI2.Models.Documents
{
    public class DocumentViewModel
    {
        public long Id { get; set; }
        public bool OutDocument { get; set; }
        public bool HasDocumentFile { get; set; }
        public string DocumentFileType { get; set; }
        [Required]
        public string Notes { get; set; }
        [Required]
        public string Comment { get; set; }
        public DateTime Created { get; set; }
        public bool IsProtocol { get; set; }
        [Required]
        public long CompanyId { get; set; }
        public string CompanyName { get; set; }
        [Required]
        public long DocumentTypeId { get; set; }
        public string DocumentType { get; set; }
        public IFormFile File { get; set; }
    }
}
