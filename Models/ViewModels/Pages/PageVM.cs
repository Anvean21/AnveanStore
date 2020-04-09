using Store.Models.Data;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using System.Web.Mvc;

//Получает данные и в конструкторе присвает значение модели которая пойдет на представление
namespace Store.Models.ViewModels.Pages
{
    public class PageVM
    {
        public PageVM()
        {
            //конструктор по умолчанию если не получится получить параметры PagesDTO
        }
        public PageVM(PagesDTO row)
        {
            //передача данных
            Id = row.Id;
            Title = row.Title;
            Slug = row.Slug;
            Body = row.Body;
            Sorting = row.Sorting;
            HasSidebar = row.HasSidebar;
        }
        public int Id { get; set; }
        // защита от дураков ВАЛИДАЦИЯ
        [Required]
        [StringLength(50,MinimumLength =3)]
        public string Title { get; set; }
        public string Slug { get; set; }
        [Required]
        [StringLength(int.MaxValue, MinimumLength = 5)]
        [AllowHtml]
        public string Body { get; set; }
        public int Sorting { get; set; }
        [Display(Name = "Sidebar")]
        public bool HasSidebar { get; set; }
    }
}