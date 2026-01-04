using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Services.Interfaces;
using Services;
using Models.Interfaces;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace AppRazor.Pages;

public class OverviewModel : PageModel
{

    readonly IFriendsService _service;
    public List<IFriend>? Friends { get; set; }
    public List<IFriend>? NewFriends { get; set; }

    public List<IFriend>? AllFriends { get; set; }



    public List<string>? Countries { get; set; }

    [BindProperty]
    public string filter { get; set; } = null;

    public bool UseSeed { get; set; } = true;

    public int NumberOfFriends { get; set; }

    public int NumberOfPets { get; set; }


    public int NrOfPages { get; set; }
    public int PageSize { get; } = 10;

    public int ThisPageNr { get; set; } = 0;
    public int PrevPageNr { get; set; } = 0;
    public int NextPageNr { get; set; } = 0;
    public int NrVisiblePages { get; set; } = 0;



    public async Task<IActionResult> OnGet()
    {

        // Letar efter pagenr i URL.
        if (int.TryParse(Request.Query["pagenr"], out int pagenr))
        {
            ThisPageNr = pagenr;
        }

        var data = await _service.ReadFriendsAsync(UseSeed, false, filter, ThisPageNr, PageSize);
        var createdData = await _service.ReadFriendsAsync(false, false, filter, ThisPageNr, PageSize);
        NewFriends = createdData.PageItems;
        Friends = data.PageItems;

        Friends.AddRange(NewFriends);

        UpdatePagination(data.DbItemsCount);


        return Page();
    }

    // Can search on friends firstname, lastname or cities.  
    public async Task<IActionResult> OnPostSearch()
    {

        var data = await _service.ReadFriendsAsync(UseSeed, false, filter, ThisPageNr, PageSize);
        var createdData = await _service.ReadFriendsAsync(false, false, filter, ThisPageNr, PageSize);

        Friends = data.PageItems;
        NewFriends = createdData.PageItems;

        Friends.AddRange(NewFriends);


        UpdatePagination(data.DbItemsCount);

        return Page();
    }


    private void UpdatePagination(int nrOfItems)
    {

        NrOfPages = (int)Math.Ceiling((double)nrOfItems / PageSize);
        PrevPageNr = Math.Max(0, ThisPageNr - 1);
        NextPageNr = Math.Min(NrOfPages - 1, ThisPageNr + 1);
        NrVisiblePages = Math.Min(10, NrOfPages);
    }

    public async Task<IActionResult> OnPostDeleteFriend(Guid friendId)
    {

        if (friendId != Guid.Empty)
        {
            await _service.DeleteFriendAsync(friendId);

            var data = await _service.ReadFriendsAsync(UseSeed, false, filter, ThisPageNr, PageSize);
            Friends = data.PageItems;
            NumberOfFriends = data.DbItemsCount;

            UpdatePagination(data.DbItemsCount);
        }


        return RedirectToPage("Overview");

    }



    public OverviewModel(IFriendsService service)
    {
        _service = service;
    }

}