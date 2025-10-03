enum TradeStatus
{
    Pending = 0,
    Accepted = 1,
    Denied = 2,
    Completed = 3
}

class Trade
{
    public int Id;
    public int ItemId;
    public string FromUser;  // requester
    public string ToUser;    // current owner
    public TradeStatus Status;

    public Trade(int id, int itemId, string fromUser, string toUser)
    {
        Id = id; ItemId = itemId; FromUser = fromUser; ToUser = toUser; Status = TradeStatus.Pending;
    }
}
