using MessagePack;
using Microsoft.AspNetCore.Components.Forms;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Net.Mime;
using System.Security.AccessControl;
using System.Xml.Linq;
using TDList.Enums;
using TDList.Models;

namespace TDList.Models
{
    public class Goal
    {
        public int Id { get; set; }
        [Required]
        [Display(Name = "Название")]
        public string Name { get; set; }
        [Required]
        [Display(Name = "Приоритет")]
        public Priority Priority { get; set; }
        public int? PriorityValue { get; set; }
        [Display(Name = "Статус")]
        public string Status { get; set; }
        public int? StatusValue { get; set; }
        [Display(Name = "Описание")]
        public string Description { get; set; }
        [Display(Name = "Исполнитель")]
        public string UserName { get; set; }
        [Display(Name = "Дата создания")]
        public DateTime? Created { get; set; }
        [Display(Name = "Дата начала")]
        public DateTime? Started { get; set; }
        [Display(Name = "Дата окончания")]
        public DateTime? Ended { get; set; }

        public Goal()
        {
            Created = DateTime.Now;
            Status = "Новая";
           
        }
    }
}







   


