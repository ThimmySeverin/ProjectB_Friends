using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Services.Interfaces;

namespace AppRazor.Pages;

public class SeedModel : PageModel
{
    private readonly ILogger<SeedModel> _logger;

    readonly IAdminService _adminService;

    [BindProperty]
    public int CurrentSeeded { get; set; }

    public SeedModel(ILogger<SeedModel> logger, IAdminService adminService)
    {
        _logger = logger;
        _adminService = adminService;
    }

    public int AmountOfFriends => friendAmount().Result;

    private async Task<int> friendAmount()
    {
        var f = await _adminService.GuestInfoAsync();
        return f.Item.Db.NrSeededFriends + f.Item.Db.NrUnseededFriends;
    }

    public void OnGet()
    {
        return;
    }

    public async Task<IActionResult> OnPost()
    {
        await _adminService.SeedAsync(CurrentSeeded);

        return Page();
    }
}

