using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.ComponentModel.DataAnnotations;

using Services.Interfaces;
using Services;
using Models.Interfaces;
using AppRazor.SeidoHelpers;

using Microsoft.AspNetCore.Mvc.Rendering;
using Models.DTO;
using Models;

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

    [BindProperty]
    public FriendIM FriendInput { get; set; }

    [BindProperty]
    public string PageHeader { get; set; }



    public ModelValidationResult ValidationResult { get; set; } = new ModelValidationResult(false, null, null);
    public List<SelectListItem> AvailablePets { set; get; } = new List<SelectListItem>().PopulateSelectList<AnimalKind>();
    public List<SelectListItem> AvailableMood { set; get; } = new List<SelectListItem>().PopulateSelectList<AnimalMood>();
    public enum StatusIM { Unknown, Unchanged, Inserted, Modified, Deleted }

    public async Task<IActionResult> OnGet()
    {
        if (Guid.TryParse(Request.Query["id"], out Guid friendId))
        {
            var data = await _friendService.ReadFriendAsync(friendId, false);

            FriendInput = new FriendIM(data.Item);
            PageHeader = $"Edit details of {FriendInput.FirstName} {FriendInput.LastName}";

        }
        else
        {
            FriendInput = new FriendIM();
            FriendInput.StatusIM = StatusIM.Inserted;
            FriendInput.Birthday = null;

            PageHeader = "Create a new friend";
        }

        return Page();
    }


    public IActionResult OnPostAddQuote()
    {
        string[] keys = {"FriendInput.NewQuote.QuoteText",
                        "FriendInput.NewQuote.QuoteAuthor"};

        if (!ModelState.IsValidPartially(out ModelValidationResult validationResult, keys))
        {
            ValidationResult = validationResult;
            return Page();
        }

        FriendInput.NewQuote.StatusIM = StatusIM.Inserted; // Status inserted, läggs senare till i databas
        FriendInput.NewQuote.QuoteId = Guid.NewGuid(); // Tillfälligt guid, för ex. radera innan man hunnit spara

        FriendInput.Quotes.Add(new QuoteIM(FriendInput.NewQuote)); // Tar inlagd quote, lägger i copy konstruktor, sedan i listan som tillhör vännen. 

        FriendInput.NewQuote = new QuoteIM(); // tömmer input fält, gör att man kan lägga till fler quotes när den tidigare väl är lagd i listan ovan. 

        return Page();
    }

    public IActionResult OnPostEditQuote([FromQuery] Guid quoteId)
    {
        var quoteindx = FriendInput.Quotes.FindIndex(q => q.QuoteId == quoteId);
        string[] keys = { $"FriendInput.Quotes[{quoteindx}].editQuoteText",
                        $"FriendInput.Quotes[{quoteindx}].editQuoteAuthor"};

        if (!ModelState.IsValidPartially(out ModelValidationResult validationResult, keys))
        {
            ValidationResult = validationResult;
            return Page();
        }

        var q = FriendInput.Quotes.First(id => id.QuoteId == quoteId);
        if (q.StatusIM != StatusIM.Inserted)
        {
            q.StatusIM = StatusIM.Modified;
        }

        q.QuoteText = q.editQuoteText;
        q.QuoteAuthor = q.editQuoteAuthor;

        return Page();
    }

    public IActionResult OnPostDeleteQuote(Guid id)
    {
        FriendInput.Quotes.First(q => q.QuoteId == id).StatusIM = StatusIM.Deleted;
        return Page();
    }

    public IActionResult OnPostAddPet()
    {
        string[] keys = { "FriendInput.NewPet.AnimalKind", "FriendInput.NewPet.AnimalMood", "FriendInput.NewPet.AnimalName" };

        if (!ModelState.IsValidPartially(out ModelValidationResult validationResult, keys))
        {
            ValidationResult = validationResult;
            return Page();
        }

        FriendInput.NewPet.StatusIM = StatusIM.Inserted; // Status inserted, läggs senare till i databas
        FriendInput.NewPet.PetId = Guid.NewGuid(); // Tillfälligt guid, för ex. radera innan man hunnit spara

        FriendInput.Pets.Add(new PetIM(FriendInput.NewPet)); // Tar inlagd quote, lägger i copy konstruktor, sedan i listan som tillhör vännen. 

        FriendInput.NewPet = new PetIM(); // tömmer input fält, gör att man kan lägga till fler quotes när den tidigare väl är lagd i listan ovan. 

        return Page();
    }

    public IActionResult OnPostEditPet([FromQuery] Guid petId)
    {
        var petindx = FriendInput.Pets.FindIndex(p => p.PetId == petId);
        string[] keys = { $"FriendInput.Pets[{petindx}].editAnimalName",
                        $"FriendInput.Pets[{petindx}].editAnimalMood",
                        $"FriendInput.Pets[{petindx}].editAnimalKind"};

        if (!ModelState.IsValidPartially(out ModelValidationResult validationResult, keys))
        {
            ValidationResult = validationResult;
            return Page();
        }

        var p = FriendInput.Pets.FirstOrDefault(p => p.PetId == petId);
        if (p.StatusIM != StatusIM.Inserted)
        {
            p.StatusIM = StatusIM.Modified;
        }

        p.AnimalName = p.editAnimalName;
        p.AnimalKind = p.editAnimalKind;
        p.AnimalMood = p.editAnimalMood;

        return Page();
    }

    public IActionResult OnPostDeletePet(Guid id)
    {
        FriendInput.Pets.First(p => p.PetId == id).StatusIM = StatusIM.Deleted;

        return Page();
    }

    public IActionResult OnPostEditAddress([FromQuery] Guid AddressId) // Kolla hur denna körs, och se så du fattar OnSaveAddress
    {
        string[] keys = { "FriendInput.Address.editStreetAddress",
                            "FriendInput.Address.editZipCode",
                            "FriendInput.Address.editCity",
                            "FriendInput.Address.editCountry"};

        if (!ModelState.IsValidPartially(out ModelValidationResult validationResult, keys))
        {
            ValidationResult = validationResult;
            return Page();
        }


        ModelState.Remove("FriendInput.Address.StatusIM");
        ModelState.Remove("FriendInput.Address.StreetAddress");
        ModelState.Remove("FriendInput.Address.City");
        ModelState.Remove("FriendInput.Address.ZipCode");
        ModelState.Remove("FriendInput.Address.Country");

        FriendInput.Address.AddressId = AddressId;

        FriendInput.Address.StreetAddress = FriendInput.Address.editStreetAddress;
        FriendInput.Address.City = FriendInput.Address.editCity;
        FriendInput.Address.ZipCode = FriendInput.Address.editZipCode;
        FriendInput.Address.Country = FriendInput.Address.editCountry;

        if (AddressId == Guid.Empty)
        {
            FriendInput.Address.StatusIM = StatusIM.Inserted;
        }
        else
        {
            FriendInput.Address.StatusIM = StatusIM.Modified;
        }

        return Page();
    }

    public async Task<IActionResult> OnPostSave() // spara 
    {
        string[] keys = {   "FriendInput.Firstname",
                            "FriendInput.Lastname"};

        if (!ModelState.IsValidPartially(out ModelValidationResult validationResult, keys))
        {
            ValidationResult = validationResult;
            return Page();
        }

        if (FriendInput.StatusIM == StatusIM.Inserted)
        {
            var newFriend = await _friendService.CreateFriendAsync(FriendInput.CreateDTO()); // Skapar en ny vän från create metoden
            FriendInput.FriendId = newFriend.Item.FriendId;
        }


        await SaveAddress();

        await SaveQuotes();

        var friend = await SavePets();
        friend = FriendInput.UpdateFriend(friend);

        await _friendService.UpdateFriendAsync(new FriendCuDto(friend));


        return Redirect($"~/ViewFriend?id={FriendInput.FriendId}");
    }


    public async Task<IFriend> SaveAddress()
    {

        var updatedFriend = await _friendService.ReadFriendAsync(FriendInput.FriendId, false);


        if (FriendInput.Address.StatusIM == StatusIM.Inserted)
        {
            var newAddress = FriendInput.Address.CreateDTO();

            newAddress.FriendsId = new List<Guid> { updatedFriend.Item.FriendId };

            await _addService.CreateAddressAsync(newAddress);
        }
        else
        {
            
            var currentAddress = updatedFriend.Item.Address;
            if (currentAddress != null)
            {
                currentAddress = FriendInput.Address.UpdateAddress(currentAddress);
                await _addService.UpdateAddressAsync(new AddressCuDto(currentAddress));
            }

        }



        var result = await _friendService.ReadFriendAsync(FriendInput.FriendId, false);
        return result.Item;


    }




    public async Task<IFriend> SavePets()
    {
        var deletedPets = FriendInput.Pets.FindAll(p => p.StatusIM == StatusIM.Deleted);
        foreach (var pet in deletedPets)
        {
            await _petService.DeletePetAsync(pet.PetId); // Raderar pet
        }
        await _friendService.ReadFriendAsync(FriendInput.FriendId, false); // Uppdaterar sidan




        var newPets = FriendInput.Pets.FindAll(p => p.StatusIM == StatusIM.Inserted); // Hittar alla som är inserted
        foreach (var pet in newPets)
        {
            var newPet = pet.PetCreate(); // För alla modifed pet, skapa dem, deras id blir null.

            newPet.FriendId = FriendInput.FriendId; // Kopplar det skapade pet till vännen som editeras
            await _petService.CreatePetAsync(newPet); // Skapar nytt pet, här får den även ett guid
        }
        var updatedFriend = await _friendService.ReadFriendAsync(FriendInput.FriendId, false); // Uppdaterar sidan igen.



        var modPet = FriendInput.Pets.FindAll(p => p.StatusIM == StatusIM.Modified); // Hittar alla som är modified.
        foreach (var pet in modPet)
        {
            var m = updatedFriend.Item.Pets.First(p => p.PetId == pet.PetId); // Kollar så att modiferade petid redan finns hos någon av vännens djur. 

            m = pet.UpdatePet(m); // Uppdaterar djuret med det som skrivs in. Pet id ändras ej. 

            m.Friend = updatedFriend.Item;
            await _petService.UpdatePetAsync(new PetCuDto(m)); // Uppdaterar det skapade djuret
        }

        return updatedFriend.Item;

    }


    public async Task<IFriend> SaveQuotes()
    {
        var deletedQuote = FriendInput.Quotes.FindAll(q => q.StatusIM == StatusIM.Deleted);
        foreach (var quote in deletedQuote)
        {
            await _quoteService.DeleteQuoteAsync(quote.QuoteId);
        }

        await _friendService.ReadFriendAsync(FriendInput.FriendId, false);




        var newQuotes = FriendInput.Quotes.FindAll(q => q.StatusIM == StatusIM.Inserted);
        foreach (var quote in newQuotes)
        {
            var newQuote = quote.QuoteCreate();

            newQuote.FriendsId = [FriendInput.FriendId];
            await _quoteService.CreateQuoteAsync(newQuote);
        }

        var updatedFriend = await _friendService.ReadFriendAsync(FriendInput.FriendId, false);





        var modquote = FriendInput.Quotes.FindAll(q => q.StatusIM == StatusIM.Modified);
        foreach (var quote in modquote)
        {
            var m = updatedFriend.Item.Quotes.First(f => f.QuoteId == quote.QuoteId);

            m = quote.UpdateQuote(m);

            m.Friends = [updatedFriend.Item];
            await _quoteService.UpdateQuoteAsync(new QuoteCuDto(m));
        }
        return updatedFriend.Item;
    }





    public class FriendIM
    {
        public StatusIM StatusIM { get; set; }
        public Guid FriendId { get; set; }

        [Required(ErrorMessage = "You must provide a firstname")]
        public string? FirstName { get; set; }

        [Required(ErrorMessage = "You must provide a lastname")]
        public string? LastName { get; set; }



        [Required(ErrorMessage = "Add a birthday in format: yyyy-mm-dd")]
        public DateTime? Birthday { get; set; }

        public AddressIM Address { get; set; } = new AddressIM();
        public List<QuoteIM> Quotes { get; set; } = new List<QuoteIM>();
        public List<PetIM> Pets { get; set; } = new List<PetIM>();

        public FriendIM() { }

        public FriendIM(IFriend model)
        {
            StatusIM = StatusIM.Unchanged;
            FriendId = model.FriendId;
            FirstName = model.FirstName;
            LastName = model.LastName;
            Birthday = model.Birthday;

            Address = model.Address != null ? new AddressIM(model.Address) : new AddressIM();

            Quotes = model.Quotes?.Select(q => new QuoteIM(q)).ToList();
            Pets = model.Pets?.Select(p => new PetIM(p)).ToList();

        }

        public IFriend UpdateFriend(IFriend model)
        {
            model.FirstName = this.FirstName;
            model.LastName = this.LastName;
            model.Birthday = this.Birthday;
            return model;
        }

        public FriendCuDto CreateDTO() => new()
        {
            FriendId = null,
            FirstName = this.FirstName,
            LastName = this.LastName,
            Birthday = this.Birthday
        };

        public QuoteIM NewQuote { get; set; } = new QuoteIM();
        public PetIM NewPet { get; set; } = new PetIM();

    }

    public class AddressIM
    {
        public StatusIM StatusIM { get; set; }
        public Guid AddressId { get; set; }

        public string? StreetAddress { get; set; } = string.Empty;
        public int? ZipCode { get; set; }
        public string? City { get; set; } = string.Empty;
        public string? Country { get; set; } = string.Empty;

        public string? editStreetAddress { get; set; }
        public int? editZipCode { get; set; }
        public string? editCity { get; set; }
        public string? editCountry { get; set; }

        public AddressIM() { }

        public AddressIM(AddressIM org)
        {
            StreetAddress = org.StreetAddress;
            ZipCode = org.ZipCode;
            City = org.City;
            Country = org.Country;

            editStreetAddress = org.StreetAddress;
            editZipCode = org.ZipCode;
            editCity = org.City;
            editCountry = org.Country;
        }

        public AddressIM(IAddress model)
        {
            StatusIM = StatusIM.Unchanged;
            AddressId = model.AddressId;
            StreetAddress = editStreetAddress = model.StreetAddress;
            ZipCode = editZipCode = model.ZipCode;
            City = editCity = model.City;
            Country = editCountry = model.Country;
        }

        public IAddress UpdateAddress(IAddress model)
        {
            model.AddressId = this.AddressId;
            model.StreetAddress = this.StreetAddress;
            model.City = this.City;
            model.Country = this.Country;
            model.ZipCode = this.ZipCode ?? 0;
            return model;
        }

        public AddressCuDto CreateDTO() => new()
        {
            AddressId = null,
            StreetAddress = this.editStreetAddress,
            City = this.editCity,
            Country = this.editCountry,
            ZipCode = this.editZipCode ?? 0
        };
    }

    public class QuoteIM
    {
        public StatusIM StatusIM { get; set; }
        public Guid QuoteId { get; set; }

        [Required(ErrorMessage = "You must enter a quote")]
        public string? QuoteText { get; set; }

        [Required(ErrorMessage = "You must enter a author")]
        public string? QuoteAuthor { get; set; }


        [Required(ErrorMessage = "You must enter a quote")]
        public string editQuoteText { get; set; }

        [Required(ErrorMessage = "You must enter a author")]
        public string editQuoteAuthor { get; set; }


        public QuoteIM() { }
        public QuoteIM(QuoteIM org)
        {
            StatusIM = org.StatusIM;
            QuoteId = org.QuoteId;
            QuoteText = org.QuoteText;
            QuoteAuthor = org.QuoteAuthor;

            editQuoteText = org.QuoteText;
            editQuoteAuthor = org.QuoteAuthor;

        }

        public QuoteIM(IQuote model)
        {
            StatusIM = StatusIM.Unchanged;
            QuoteId = model.QuoteId;
            QuoteText = editQuoteText = model.QuoteText;
            QuoteAuthor = editQuoteAuthor = model.Author;


        }

        public IQuote UpdateQuote(IQuote model)
        {
            model.QuoteId = this.QuoteId;
            model.QuoteText = this.QuoteText;
            model.Author = this.QuoteAuthor;
            return model;
        }

        public QuoteCuDto QuoteCreate() => new()
        {
            QuoteId = null,
            Quote = this.QuoteText,
            Author = this.QuoteAuthor
        };

    }

    public class PetIM
    {
        public StatusIM StatusIM { get; set; }
        public Guid PetId { get; set; }

        [Required(ErrorMessage = "You must enter a name for the pet")]
        public string? AnimalName { get; set; }

        [Required(ErrorMessage = "You must enter kind of the pet")]
        public AnimalKind? AnimalKind { get; set; }

        [Required(ErrorMessage = "You must enter a mood for the pet")]
        public AnimalMood? AnimalMood { get; set; }


        [Required(ErrorMessage = "You must enter a name for the pet")]
        public string? editAnimalName { get; set; }

        [Required(ErrorMessage = "You must enter kind of the pet")]
        public AnimalKind? editAnimalKind { get; set; }

        [Required(ErrorMessage = "You must enter a mood for the pet")]
        public AnimalMood? editAnimalMood { get; set; }


        public PetIM() { }
        public PetIM(PetIM org)
        {
            StatusIM = org.StatusIM;
            PetId = org.PetId;
            AnimalName = org.AnimalName;
            AnimalKind = org.AnimalKind;
            AnimalMood = org.AnimalMood;

            editAnimalName = org.AnimalName;
            editAnimalKind = org.AnimalKind; // Fungerar som en "kladdlapp", så att det finns ett värde vid edit som kan komma tillbaka vid undo. 
            editAnimalMood = org.AnimalMood;
        }
        public PetIM(IPet model)
        {

            StatusIM = StatusIM.Unchanged;
            PetId = model.PetId;
            AnimalName = editAnimalName = model.Name;
            AnimalKind = editAnimalKind = model.Kind;
            AnimalMood = editAnimalMood = model.Mood;
        }

        public IPet UpdatePet(IPet model)
        {
            model.PetId = this.PetId;
            model.Name = this.editAnimalName;
            model.Kind = this.editAnimalKind.Value;
            model.Mood = this.editAnimalMood.Value;
            return model;
        }

        public PetCuDto PetCreate() => new()
        {
            PetId = null,
            Name = this.AnimalName,
            Kind = this.AnimalKind.Value,
            Mood = this.AnimalMood.Value,
        };
    }
}