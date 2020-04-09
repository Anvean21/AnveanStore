using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Data.Entity;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

//Модель для связи с таблицей
namespace Store.Models.Data
{
    [Table("tblPages")]
    public class PagesDTO
    {
        [Key] //первичный ключ, для безопасности, мвс сам реагирует на Id
        public int Id { get; set; }
        public string Title { get; set; }
        public string Slug { get; set; }
        public string Body { get; set; }
        public int Sorting { get; set; }
        public bool HasSidebar { get; set; }

    }
}