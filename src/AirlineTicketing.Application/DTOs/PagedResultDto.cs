namespace AirlineTicketing.Application.DTOs;

public class PagedResultDto<T>
{
    public List<T> Items { get; set; } = new();
    public int Page { get; set; }
    public int Size { get; set; }
    public int TotalCount { get; set; }
    public int TotalPages { get; set; }
}