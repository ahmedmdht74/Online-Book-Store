using Book_Store.Models;
using System.ComponentModel.DataAnnotations;

namespace Book_Store.View_Models.Dashboard
{
    public class CityListVM
    {
        public string searchtext { get; set; }
        public int orderby { get; set; }
        public int CityCount { get; set; }
        public List<CityDetailInCityListVM> CityDetailInCityLists { get; set; }
        public List<State> StateList { get; set; }

        //Operation
        [Display(Name = "City"), Required(ErrorMessage = "*required*")]
        public string ModalCityInput { get; set; }
        [Display(Name = "Satet"), Required(ErrorMessage = "*Assign state to city ...*")]
        public int ModalStateIdInput { get; set; }
    }


    public class CityDetailInCityListVM
    {
        public int ID { get; set; }
        public string Name { get; set; }
        public string StateName { get; set; }
        public int StateId { get; set; }
        public int DeliveryNumber { get; set; }

    }
}
