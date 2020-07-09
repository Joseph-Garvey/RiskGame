using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using RiskGame.Game;
using System.Windows.Markup;

namespace RiskGame
{
    public abstract class BaseConverter : MarkupExtension
    {
        public override object ProvideValue(IServiceProvider serviceProvider)
        {
            return this;
        }
    }
    [ValueConversion(typeof(DateTime), typeof(String))]
    public class DateConverter : BaseConverter , IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            DateTime date = (DateTime)value;
            return date.ToShortDateString();
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            string strValue = value as string;
            DateTime resultDateTime;
            if (DateTime.TryParse(strValue, out resultDateTime))
            {
                return resultDateTime;
            }
            return DependencyProperty.UnsetValue;
        }
    }
    [ValueConversion(typeof(int), typeof(String))]
    public class IntToString : BaseConverter, IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return ((int)value).ToString();
        }
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            int.TryParse((String)value, out int i);
            return i;
        }
    }
    [ValueConversion(typeof(Enum), typeof(String))]
    public class EnumConverter : BaseConverter, IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value.ToString();
        }
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if(value is GameState)
            {
                switch (value)
                {
                    case "None":
                    case 0:
                        return GameState.None;
                    case "Attacking":
                    case 1:
                        return GameState.Attacking;
                    case "Conquer":
                    case 2:
                        return GameState.Conquer;
                    case "Move":
                    case 3:
                        return GameState.Move;
                    case "PlacingArmy":
                    case 4:
                        return GameState.PlacingArmy;
                    case "InitialArmyPlace":
                    case 5:
                        return GameState.InitialArmyPlace;
                }
            }
            else if(value is GameMode)
            {
                switch (value)
                {
                    case "NewRisk":
                    case 0:
                        return GameMode.NewRisk;
                    case "Classic":
                    case 1:
                        return GameMode.Classic;
                }
            }
            else if (value is GameMap)
            {
                switch (value)
                {
                    case "Default":
                    case 0:
                        return GameMap.Default;
                }
            }
            return DependencyProperty.UnsetValue;
        }
    }
}
