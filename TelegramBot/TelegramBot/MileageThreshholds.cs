﻿using System;
using System.Collections.Generic;

class MileageThresholds
{
    public static Dictionary<string, Dictionary<string, int>> PartMileageThresholds = new Dictionary<string, Dictionary<string, int>>
    {
         {
            "Бензиновый",
            new Dictionary<string, int>
            {
                { "заменить моторное масло, масляный фильтр, салонный фильтр", 8000 },
                { "заменить тормозные колодки, щетки стеклоочистителя", 30000 },
                { "заменить масло в коробке передач, свечи зажигания, проверить состояние аккумуляторной батареи", 50000 },
                { "заменить топливный фильтр, топливный насос, тормозные диски, катушки зажигания", 80000},
                { "заменить ремень ГРМ", 90000},
                { "заменить охлаждающую жидкость, тормозную жидкость, произвести ремонт или замену генератора", 160000},

            }
        },
        {
            "Дизельный",
            new Dictionary<string, int>
            {
                { "заменить моторное масло, масляный фильтр, салонный фильтр", 8000 },
                { "заменить тормозные колодки, щетки стеклоочистителя, ", 30000 },
                { "заменить масло в коробке передач, проверить состояние аккумуляторной батареи", 50000 },
                { "заменить топливный фильтр, топливный насос, тормозные диски, свечи накаливания", 80000},
                { "заменить ремень ГРМ", 90000},
                { "заменить охлаждающую жидкость, тормозную жидкость, произвести ремонт или замену генератора", 160000},
            }
        },
        {
            "Электро",
            new Dictionary<string, int>
            {
                { "заменить салонный фильтр, щетки стеклоочистителя", 10000 },
                { "заменить тормозные колодки", 40000 },
                { "заменить тормозные диски", 60000 },
                { "заменить охлаждающую жидкость, тормозную жидкость", 125000 },
                { "заменить аккумуляторную батарею" , 130000}

            }
        }
    };
}
