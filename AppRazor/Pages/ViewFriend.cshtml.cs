using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Services.Interfaces;
using Services;
using Models.Interfaces;

namespace AppRazor.Pages;

public class ViewFriendModel : PageModel
{


    readonly IFriendsService _service;
    public IFriend Friend { get; set; }

    public async Task<IActionResult> OnGet()
    {
        Guid _friendId = Guid.Parse(Request.Query["id"]);
        Friend = (await _service.ReadFriendAsync(_friendId, false)).Item;


     

        return Page();

    }


    public ViewFriendModel(IFriendsService service)
    {
        _service = service;

    }

}