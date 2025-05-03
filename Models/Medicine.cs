using SQLite;

namespace PillTime.Models
{
    public class Medicine
    {
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }

        public string Name { get; set; }                  // Название лекарства
        public int PackageCount { get; set; }            // Кол-во упаковок или дней, на сколько хватит
        public string DosePerIntake { get; set; }        // Доза за приём (текст)
        public string SpecialInstructions { get; set; } // Особые указания
        public bool IsTakenToday { get; set; }          // Отмечено ли на сегодня

        public string Unit { get; set; }               // Единица измерения
        public int IntakesPerDay { get; set; }         // Количество приёмов в день

        public string IntakeTimesString { get; set; }  // Время приёмов (сохраняем строкой)
        public string IntakeDosesString { get; set; }  // Дозы приёмов (сохраняем строкой)

        public double StockAmount { get; set; }        // Запас препарата в единицах
        public int DaysAvailable { get; set; }         // Кол-во дней, на сколько хватит
        

        public double MaxDailyDosePerKg { get; set; } // Максимальная суточная доза (мг/кг)


        [Ignore]
        public string[] IntakeTimes
        {
            get => string.IsNullOrEmpty(IntakeTimesString)
                ? new string[0]
                : IntakeTimesString.Split(';');
            set => IntakeTimesString = value != null
                ? string.Join(";", value)
                : string.Empty;
        }

        [Ignore]
        public string[] IntakeDoses
        {
            get => string.IsNullOrEmpty(IntakeDosesString)
                ? new string[0]
                : IntakeDosesString.Split(';');
            set => IntakeDosesString = value != null
                ? string.Join(";", value)
                : string.Empty;
        }

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
        [Ignore]
        public string StockDescription
        {
            get
            {
                if (Unit == "мг")
                    return $"{StockAmount} мг";
                return $"{StockAmount} {Unit}";
            }
        }

        [Ignore]
        public string NextDoseDescription
        {
            get
            {
                return IntakeDoses.Length > 0 ? $"{IntakeDoses[0]}" : "-";
            }
        }

        [Ignore]
        public double PerDoseAmount
        {
            get
            {
                var firstDose = IntakeDoses.FirstOrDefault();
                if (firstDose == null)
                    return 0;

                var parts = firstDose.Split(' ');
                if (double.TryParse(parts[0]?.Replace(',', '.'), System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out double dose))
                    return dose;

                return 0;
            }
        }

        
        public void RecalculateDaysAvailable()
        {
            if (Unit == "мг")
            {
                // Самостоятельный курс
                double weight = Preferences.Get("Weight", 0.0);
                double maxDailyDose = weight * MaxDailyDosePerKg;

                DaysAvailable = maxDailyDose > 0
                    ? (int)Math.Floor(StockAmount / maxDailyDose)
                    : 0;
            }
            else
            {
                // По назначению врача — суммируем дозы за день
                double dailyUsage = 0;
                foreach (var doseStr in IntakeDoses)
                {
                    var parts = doseStr.Split(' ');
                    if (double.TryParse(parts[0]?.Replace(',', '.'), System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out double dose))
                        dailyUsage += dose;
                }

                DaysAvailable = dailyUsage > 0
                    ? (int)Math.Floor(StockAmount / dailyUsage)
                    : 0;
            }
        }

    }

}
