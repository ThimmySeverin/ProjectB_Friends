using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.ComponentModel.DataAnnotations;

using Services.Interfaces;
using Services;
using Models.Interfaces;
using AppRazor.SeidoHelpers;

using Microsoft.AspNetCore.Mvc.Rendering;

namespace AppRazor.Pages;

public class EditFriendModel : PageModel
{

    readonly IFriendsService _friendService;
    readonly IAddressesService _addService;
    readonly IPetsService _petService;
    readonly IQuotesService _quoteService;

    public EditFriendModel(IFriendsService friendsService, IAddressesService addService,
    IPetsService petService, IQuotesService quoteService)
    {
        _friendService = friendsService;
        _addService = addService;
        _petService = petService;
        _quoteService = quoteService;
    }

    public List<SelectListItem> AvailablePets { set; get; } = new List<SelectListItem>().PopulateSelectList<AnimalKind>();
    public List<SelectListItem> AvailableMood { set; get; } = new List<SelectListItem>().PopulateSelectList<AnimalMood>();

    public enum StatusIM { Unknown, Unchanged, Inserted, Modified, Deleted }

    public async Task<IActionResult> OnGet()
    {
        if (Guid.TryParse(Request.Query["id"], out Guid friendId))
        {
            var data = await _friendService.ReadFriendAsync(friendId, false);

        }

        return Page();
    }

    public class FriendIM
    {
        public StatusIM StatusIM { get; set; }
        public Guid FriendId { get; set; }

        [Required(ErrorMessage = "You must provide a firstname")]
        public string? FirstName { get; set; }

        [Required(ErrorMessage = "You must provide a lastname")]
        public string? LastName { get; set; }

        [Range(1920, 2025, ErrorMessage = "Select a year between 1920 and 2025")]
        public DateTime Birthday { get; set; }

        public AddressIM Address { get; set; } = new AddressIM();
        public List<QuoteIM> Quotes { get; set; } = new List<QuoteIM>();
        public List<PetIM> Pets { get; set; } = new List<PetIM>();



    }

    public class AddressIM
    {

    }

    public class QuoteIM
    {

    }

    public class PetIM
    {

    }
}