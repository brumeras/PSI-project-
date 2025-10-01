Application can be interacted with using some sort of interface
Reiškia egzistuoja kažkoks UI, ne viskas eina per komandinę eilutę.
Įgyvendinta, čia tam naudojam .razor failus
2. Creating and using your own class, struct, record and enum
   Objektinio programavimo C # dalykai . . CS failuose turim klasės pavyzdžių, tačiau kitų trūksta
3. Property usage in struct and class
   Basically get ir set. Klasėje users naudojam, struktūruose dar ne, nes struktūros neturim
4. Named and optional argument usage
   Kas tas per dalykas gerai paaiškina šitas pvz :
   public void SavePlayer(Player player, bool overwrite = false) { ... }

// usage
SavePlayer(player, overwrite: true); // named argument
SavePlayer(player); // optional argument (defaults to false)
Neturim, bet chatukas įmetė keletą suggestion kurpraverstų.
5. Extension method usage
   Trumpai tariant “this” naudojimas. Neturim. Reikia statinių klasių, jų dar irgi nėra
6. Iterating through collection the right way
   Nenaudoti for kur nereikia indeksų. Vietoj to naudoti foreach. Neturim.
7. Using a stream to load data (can be from file, web service, socket etc.)
   Neturim, bet reikia keisti paprastą skaitymą, kurįnaudojam dabar į streamus, pvz users saugoti.
8. Boxing and unboxing
   Pasirodo visai ne sumušimas ir supakavimas. Geeksforgeeks aiškina taip:
   The C# Type System contains three data types that are Value Types (int, char, etc), Reference Types (object), and Pointer Types.
   • Boxing: It is a process that converts a Value Type variable into a Reference Type variable.
   • Unboxing: This process extracts the original value type from an object and converts it back to the value type.
   Neturim
9. LINQ to Objects usage (methods or queries)
   Tokių LINQ žodelių kaip Where, Select, OrderByir t.t naudojimas. Realiai tam tikros operacijos su listais, masyvais ir kitais gaidynais. Turim to pavyzdį dirbant su usernames.
   Any pvz if (_users.Any(u => u.Username.Equals(username, StringComparison.OrdinalIgnoreCase)))
   First or default var user = _users.FirstOrDefault
10. Implement at least one of the standard .NET interfaces (IEnumerable, IComparable, IComparer, IEquatable, IEnumerator, etc.)
    Interfeisai objektų palyginimui, pvz username. Nenaudojam

