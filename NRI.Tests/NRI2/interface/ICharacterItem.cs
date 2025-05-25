public interface ICharacterItem
{
    int Id { get; set; }
    int CharacterId { get; set; }
    string Name { get; set; }
    int Quantity { get; set; }
    bool IsEquipped { get; set; }
}


