class Program
{
    static List<User> Users = new List<User>();
    static List<Item> Items = new List<Item>();
    static List<Trade> Trades = new List<Trade>();

    static int nextItemId = 1;
    static int nextTradeId = 1;

    static void Main(string[] args)
    {
        User active = null;

        while (true)
        {
            if (active == null)
            {
                Console.Clear();
                Console.WriteLine("=== Trading System ===");
                Console.WriteLine("1) Register");
                Console.WriteLine("2) Login");
                Console.WriteLine("3) Exit");
                Console.Write("Choose: ");
                string choose = Console.ReadLine();

                if (choose == "1") Register();
                else if (choose == "2") active = Login();
                else if (choose == "3") return;
                else Pause("Invalid option.");
            }
            else
            {
                Console.Clear();
                Console.WriteLine("=== Main === (User: " + active.Username + ")");
                Console.WriteLine("1) Add item");
                Console.WriteLine("2) Browse others' items");
                Console.WriteLine("3) Request trade for item");
                Console.WriteLine("4) Incoming trade requests");
                Console.WriteLine("5) Outgoing trade requests");
                Console.WriteLine("6) Accept/Deny incoming");
                Console.WriteLine("7) Completed trades (mine)");
                Console.WriteLine("8) Logout");
                Console.Write("Choose: ");
                string c = Console.ReadLine();

                if (c == "1") AddItem(active);
                else if (c == "2") ShowOthersItems(active);
                else if (c == "3") RequestTrade(active);
                else if (c == "4") ShowIncoming(active);
                else if (c == "5") ShowOutgoing(active);
                else if (c == "6") HandleIncoming(active);
                else if (c == "7") ShowCompleted(active);
                else if (c == "8") active = null;
                else Pause("Invalid option.");
            }
        }
    }

    // --- Account ---
    static void Register()
    {
        Console.Clear();
        Console.WriteLine("=== Register ===");
        Console.Write("Username: ");
        string user = (Console.ReadLine() ?? "").Trim();
        if (user.Length == 0) { Pause("Empty username."); return; }

        // Check if user exists
        for (int i = 0; i < Users.Count; i++)
        {
            if (EqualsIgnoreCase(Users[i].Username, user))
            {
                Pause("Username already exists.");
                return;
            }
        }

        Console.Write("Password: ");
        string p = Console.ReadLine() ?? "";
        Users.Add(new User(user, p));
        Pause("Registered.");
    }

    static User Login()
    {
        Console.Clear();
        Console.WriteLine("=== Login ===");
        Console.Write("Username: ");
        string username = Console.ReadLine() ?? "";
        Console.Write("Password: ");
        string password = Console.ReadLine() ?? "";

        for (int i = 0; i < Users.Count; i++)
        {
            if (EqualsIgnoreCase(Users[i].Username, username) && Users[i].Password == password)
            {
                Pause("Logged in.");
                return Users[i];
            }
        }
        Pause("Invalid credentials.");
        return null;
    }

    // --- Items ---
    static void AddItem(User user)
    {
        Console.Clear();
        Console.WriteLine("=== Add Item ===");
        Console.Write("Name: ");
        string name = (Console.ReadLine() ?? "").Trim();
        if (name.Length == 0)
        {
            Pause("Name required.");
            return;
        }
        Console.Write("Description: ");
        string description = Console.ReadLine() ?? "";

        Item item = new Item(nextItemId++, user.Username, name, description);
        Items.Add(item);
        Pause("Added item #" + item.Id + ".");
    }

    static void ShowOthersItems(User me)
    {
        Console.Clear();
        Console.WriteLine("=== Others' Items ===");
        bool any = false;
        for (int i = 0; i < Items.Count; i++)
        {
            if (!EqualsIgnoreCase(Items[i].Owner, me.Username))
            {
                Console.WriteLine("[" + Items[i].Id + "] " + Items[i].Name + " — " + Items[i].Description + " (Owner: " + Items[i].Owner + ")");
                any = true;
            }
        }
        if (!any) Console.WriteLine("(none)");
        Pause();
    }

    // --- Trades ---
    static void RequestTrade(User me)
    {
        Console.Clear();
        Console.WriteLine("=== Request Trade ===");
        Console.Write("Enter ItemId (or empty to cancel): ");
        string s = Console.ReadLine();
        if (string.IsNullOrWhiteSpace(s)) return;
        int id;
        if (!int.TryParse(s, out id)) { Pause("Invalid id."); return; }

        Item target = FindItemById(id);
        if (target == null) { Pause("Item not found."); return; }
        if (EqualsIgnoreCase(target.Owner, me.Username)) { Pause("Cannot request your own item."); return; }

        Trade t = new Trade(nextTradeId++, target.Id, me.Username, target.Owner);
        Trades.Add(t);
        Pause("Trade request #" + t.Id + " sent to " + t.ToUser + ".");
    }

    static void ShowIncoming(User me)
    {
        Console.Clear();
        Console.WriteLine("=== Incoming (Pending) ===");
        bool any = false;
        for (int i = 0; i < Trades.Count; i++)
        {
            Trade t = Trades[i];
            if (EqualsIgnoreCase(t.ToUser, me.Username) && t.Status == TradeStatus.Pending)
            {
                Item it = FindItemById(t.ItemId);
                string name = it != null ? it.Name : "(unknown item)";
                Console.WriteLine("Trade #" + t.Id + " — Item #" + t.ItemId + " \"" + name + "\" from " + t.FromUser + " — Status: " + t.Status);
                any = true;
            }
        }
        if (!any) Console.WriteLine("(none)");
        Pause();
    }

    static void ShowOutgoing(User me)
    {
        Console.Clear();
        Console.WriteLine("=== Outgoing ===");
        bool any = false;
        for (int i = 0; i < Trades.Count; i++)
        {
            Trade t = Trades[i];
            if (EqualsIgnoreCase(t.FromUser, me.Username))
            {
                Item it = FindItemById(t.ItemId);
                string name = it != null ? it.Name : "(unknown item)";
                Console.WriteLine("Trade #" + t.Id + " to " + t.ToUser + " — Item #" + t.ItemId + " \"" + name + "\" — Status: " + t.Status);
                any = true;
            }
        }
        if (!any) Console.WriteLine("(none)");
        Pause();
    }

    static void HandleIncoming(User me)
    {
        Console.Clear();
        Console.WriteLine("=== Accept/Deny Incoming ===");
        // list pending for me
        bool any = false;
        for (int i = 0; i < Trades.Count; i++)
        {
            Trade t = Trades[i];
            if (EqualsIgnoreCase(t.ToUser, me.Username) && t.Status == TradeStatus.Pending)
            {
                Item it = FindItemById(t.ItemId);
                string name = it != null ? it.Name : "(unknown item)";
                Console.WriteLine("Trade #" + t.Id + " — Item #" + t.ItemId + " \"" + name + "\" from " + t.FromUser);
                any = true;
            }
        }
        if (!any) { Pause("No pending requests."); return; }

        Console.Write("Enter TradeId: ");
        string s = Console.ReadLine();
        int tid;
        if (!int.TryParse(s, out tid)) { Pause("Invalid id."); return; }

        Trade trade = FindTradeById(tid);
        if (trade == null || !EqualsIgnoreCase(trade.ToUser, me.Username) || trade.Status != TradeStatus.Pending)
        { Pause("Trade not found or not pending."); return; }

        Console.Write("(A)ccept or (D)eny: ");
        string choice = (Console.ReadLine() ?? "").Trim().ToUpper();
        if (choice == "A")
        {
            trade.Status = TradeStatus.Accepted;

            // Transfer ownership immediately and mark completed.
            Item it = FindItemById(trade.ItemId);
            if (it != null) it.Owner = trade.FromUser;

            trade.Status = TradeStatus.Completed;
            Pause("Accepted. Ownership transferred. Trade completed.");
        }
        else if (choice == "D")
        {
            trade.Status = TradeStatus.Denied;
            Pause("Denied.");
        }
        else
        {
            Pause("Cancelled.");
        }
    }

    static void ShowCompleted(User me)
    {
        Console.Clear();
        Console.WriteLine("=== Completed (Mine) ===");
        bool any = false;
        for (int i = 0; i < Trades.Count; i++)
        {
            Trade t = Trades[i];
            if (t.Status == TradeStatus.Completed &&
                (EqualsIgnoreCase(t.FromUser, me.Username) || EqualsIgnoreCase(t.ToUser, me.Username)))
            {
                Item it = FindItemById(t.ItemId);
                string name = it != null ? it.Name : "(unknown item)";
                Console.WriteLine("Trade #" + t.Id + " — Item \"" + name + "\" between " + t.FromUser + " and " + t.ToUser + " — Completed");
                any = true;
            }
        }
        if (!any) Console.WriteLine("(none)");
        Pause();
    }

    // --- BE DEN TA BORT HELPERS SOM KAN SE LITE FÖR OÄKTA UT, EXEMPELVIS EQUALSIGNORECASE OCH PAUSE, SE OM DU VILL HA KVAR FINDITEM/FINDTRADE. DET GÅR ATT SÄTTA IN SOM LOOPAR I SYSTEMET ---
    static Item FindItemById(int id)
    {
        for (int i = 0; i < Items.Count; i++) if (Items[i].Id == id) return Items[i];
        return null;
    }

    static Trade FindTradeById(int id)
    {
        for (int i = 0; i < Trades.Count; i++) if (Trades[i].Id == id) return Trades[i];
        return null;
    }
    
    static bool EqualsIgnoreCase(string a, string b)
    {
        if (a == null || b == null) return a == b;
        return string.Equals(a, b, StringComparison.OrdinalIgnoreCase);
    }

    static void Pause(string msg = null)
    {
        if (!string.IsNullOrEmpty(msg)) Console.WriteLine(msg);
        Console.WriteLine("Press Enter to continue...");
        Console.ReadLine();
    }
}