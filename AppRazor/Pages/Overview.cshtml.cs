using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Services.Interfaces;
using Services;
using Models.Interfaces;

namespace AppRazor.Pages;

public class OverviewModel : PageModel
{

    readonly IFriendsService _service;
    public List<IFriend> Friends { get; set; }

    [BindProperty]
    public string filter { get; set; } = null;

    public bool UseSeed { get; set; } = true;


    public int NrOfPages { get; set; }
    public int PageSize { get; } = 10;

    public int ThisPageNr { get; set; } = 0;
    public int PrevPageNr { get; set; } = 0;
    public int NextPageNr { get; set; } = 0;
    public int NrVisiblePages { get; set; } = 0;



    public async Task<IActionResult> OnGet()
    {

       var data = await _service.ReadFriendsAsync(UseSeed, false, filter, ThisPageNr, PageSize);
       Friends = data.PageItems;
       

        return Page();
    }



    public OverviewModel(IFriendsService service)
    {
        _service = service;
    }

}