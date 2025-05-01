    using SQLite;

    namespace PillTime.Models
    {
        public class Medicine
        {
            [PrimaryKey, AutoIncrement]
            public int Id { get; set; }

            public string Name { get; set; }                  // Название лекарства
            public int PackageCount { get; set; }              // Количество единиц в упаковке (например, 20 таблеток)
            public string DosePerIntake { get; set; }          // Количество за один приём (например, "1 порошок")
            public int IntakesPerDay { get; set; }             // Количество приёмов в день (например, 3)
            public string TimesPerDay { get; set; }
            public string SpecialInstructions { get; set; }   // Особые указания по применению (например, "развести в воде")
            public bool IsTakenToday { get; set; }
            public string IntakeStatus { get; set; } // Например: "✔,✔,✖"



        // Виртуальное свойство (не сохраняется в базе)
        [Ignore]
            public int CourseDays
            {
                get
                {
                    if (IntakesPerDay > 0)
                        return PackageCount / IntakesPerDay;
                    return 0;
                }
            }
        }
    }