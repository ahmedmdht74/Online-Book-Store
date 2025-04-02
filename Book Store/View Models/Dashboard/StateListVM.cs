using Book_Store.Models;
using System.ComponentModel.DataAnnotations;

namespace Book_Store.View_Models.Dashboard
{
    public class StateListVM
    {
        public string searchtext { get; set; }
        public int orderby { get; set; }
        public int StateCount { get; set; }
        public List<StateDetailInStateListVM> StateDetailInStateLists { get; set; }


        //Operation
        [Display(Name = "State"),Required(ErrorMessage ="*required*")]
        public string ModalStateInput { get; set; }
    }

    public class StateDetailInStateListVM 
    {
        public int ID { get; set; }
        public string Name { get; set; }
        public int CityNumbers { get; set; }
        public int AllDeliveryNumber { get; set; }
        public List<City> Cities { get; set; }
    }
}
