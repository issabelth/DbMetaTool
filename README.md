### Do czego służy ten program?

Aplikacja to „kopiarka struktur” dla baz danych systemu Firebird. Pozwala ona w prosty sposób:
- Pobrać przepis na bazę: Wyciąga informacje o tym, jak zbudowana jest Twoja baza (tabele, pola, procedury) i zapisuje to do zwykłych plików (np. tekstowych lub SQL).
- Zbudować nową bazę: Na podstawie tych plików potrafi stworzyć identyczną, nową bazę danych w innym miejscu.
- Zaktualizować bazę: Jeśli masz już bazę, ale chcesz do niej dodać nowe elementy z przygotowanych plików, program zrobi to automatycznie.

### Jak zacząć działać?

- Wyszukaj na komputerze program Powershell.
- W pierwszej kolejności dodamy sobie alias, aby móc łatwo używać metod, bez każdorazowego pisania ścieżki.
- W Powershellu wykonaj taki skrypt: _Set-Alias DbMetaTool "<<tu_wpisz_ścieżkę_do_exe>>"_ .
  - np. _Set-Alias DbMetaTool "C:\Users\iganko\source\repos\Private\DbMetaTool\DbMetaTool\bin\Debug\net8.0\DbMetaTool.exe"_
- Super! Od teraz wystarczy, że wpiszesz DbMetaTool na początku :)

- Zróbmy jeszcze jeden krok- w folderze, gdzie jest ten plik .exe dodaj plik _ConnectionString.txt_. W środku zawrzyj swój connectionstring, zostawiając parametr Database pusty.
  - np.: _User=SYSDBA;Password=1234;DataSource=localhost;Charset=UTF8;Database=_
- Gotowe! Możemy działać :)

Aby zacząć, wpisz w Powershella komendę DbMetaTool. Wyświetlą Ci się wszystkie możliwe funkcje.

### Uwagi

Update działa jedynie dla Twoich własnych skryptów- nie są one generowane przez program.
