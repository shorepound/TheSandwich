namespace BackOfTheHouse.Services;

public interface ISandwichRepository
{
    Task<IEnumerable<SandwichDto>> GetAllAsync();
    Task<SandwichDto?> GetByIdAsync(int id);
    Task<SandwichDto> CreateAsync(SandwichDto sandwich);
    Task<bool> UpdateAsync(int id, SandwichDto sandwich);
    Task<bool> DeleteAsync(int id);
    Task<int> BackfillPricesAsync();
}

public interface IOptionsRepository
{
    Task<IEnumerable<OptionDto>> GetBreadOptionsAsync();
    Task<IEnumerable<OptionDto>> GetCheeseOptionsAsync();
    Task<IEnumerable<OptionDto>> GetDressingOptionsAsync();
    Task<IEnumerable<OptionDto>> GetMeatOptionsAsync();
    Task<IEnumerable<OptionDto>> GetToppingOptionsAsync();
}

public record SandwichDto(int Id, string Name, string? Description, decimal? Price, bool Toasted);
public record OptionDto(int Id, string Label);