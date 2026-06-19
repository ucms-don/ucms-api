namespace Ucms.Domain.Enums;

using System.ComponentModel.DataAnnotations;

/// <summary>
/// Tip produkta (qurilish materiallari va jihozlari) / Тип продукта (строительные материалы и оборудование)
/// </summary>
public enum ProductType
{
    /// <summary>
    /// Standart / По умолчанию
    /// </summary>
    [Display(Name = "По умолчанию")]
    Default = 0,

    #region Qurilish materiallari / Строительные материалы

    /// <summary>
    /// Sement va qorishma aralashmalari / Цемент и сухие смеси
    /// </summary>
    [Display(Name = "Цемент и сухие смеси")]
    Cement = 10,

    /// <summary>
    /// G'isht va blok (g'isht, gazoblok, keramoblok) / Кирпич и блоки (кирпич, газоблок, керамоблок)
    /// </summary>
    [Display(Name = "Кирпич и блоки")]
    Brick = 11,

    /// <summary>
    /// Beton va temir-beton buyumlar / Бетон и железобетонные изделия
    /// </summary>
    [Display(Name = "Бетон и ЖБИ")]
    Concrete = 12,

    /// <summary>
    /// Armatura va metallokonstruksiya / Арматура и металлоконструкции
    /// </summary>
    [Display(Name = "Арматура и металлоконструкции")]
    Rebar = 13,

    /// <summary>
    /// Pol va devor kafel-plitkalari / Напольная и настенная плитка
    /// </summary>
    [Display(Name = "Плитка (напольная/настенная)")]
    Tile = 14,

    /// <summary>
    /// Bo'yoq va lak materiallari / Лакокрасочные материалы
    /// </summary>
    [Display(Name = "Лакокрасочные материалы")]
    Paint = 15,

    /// <summary>
    /// Gipsokarton va shtukaturka materiallari / Гипсокартон и штукатурные смеси
    /// </summary>
    [Display(Name = "Гипсокартон и штукатурка")]
    Drywall = 16,

    /// <summary>
    /// Issiqlik va gidroizolyatsiya materiallari / Тепло- и гидроизоляция
    /// </summary>
    [Display(Name = "Тепло- и гидроизоляция")]
    Insulation = 17,

    /// <summary>
    /// Yog'och va pogonaj materiallari (taxta, brus, plinttus) / Пиломатериалы и погонаж
    /// </summary>
    [Display(Name = "Пиломатериалы и погонаж")]
    Lumber = 18,

    /// <summary>
    /// Tom yopish materiallari (shifer, metallochepitsa va h.k.) / Кровельные материалы
    /// </summary>
    [Display(Name = "Кровельные материалы")]
    RoofingMaterial = 19,

    #endregion

    #region Quvur va kommunikatsiyalar / Трубы и коммуникации

    /// <summary>
    /// Suv va kanalizatsiya quvurlari / Трубы водоснабжения и канализации
    /// </summary>
    [Display(Name = "Трубы водоснабжения и канализации")]
    Pipe = 30,

    /// <summary>
    /// Elektr kabel va simlari / Электрические кабели и провода
    /// </summary>
    [Display(Name = "Кабели и провода")]
    Cable = 31,

    /// <summary>
    /// Santexnika armaturalari (kran, fitting, ventil) / Сантехническая арматура (краны, фитинги, вентили)
    /// </summary>
    [Display(Name = "Сантехническая арматура")]
    PlumbingFixture = 32,

    /// <summary>
    /// Elektr jihozlari (rozetka, vyklyuchatel, light) / Электрооборудование (розетки, выключатели, светильники)
    /// </summary>
    [Display(Name = "Электрооборудование")]
    ElectricalFixture = 33,

    #endregion

    #region Asbob-uskuna va texnika / Инструменты и техника

    /// <summary>
    /// Qurilish texnikasi (kran, ekskavator va h.k.) / Строительная техника
    /// </summary>
    [Display(Name = "Строительная техника")]
    Equipment = 50,

    /// <summary>
    /// Qo'l asboblari / Ручной инструмент
    /// </summary>
    [Display(Name = "Ручной инструмент")]
    HandTool = 51,

    /// <summary>
    /// Elektr asboblari (perforator, shurupovert va h.k.) / Электроинструмент
    /// </summary>
    [Display(Name = "Электроинструмент")]
    PowerTool = 52,

    /// <summary>
    /// Lestnitsa va eshafot (qurilish zinapoyalari) / Лестницы и строительные подмостья
    /// </summary>
    [Display(Name = "Лестницы и подмостья")]
    Ladder = 53,

    /// <summary>
    /// Himoya kiyim-kechak va SIV (kaska, qo'lqop, kombinezon) / Защитная одежда и СИЗ (каска, перчатки, спецодежда)
    /// </summary>
    [Display(Name = "Защитная одежда и СИЗ")]
    ProtectiveSuit = 54,

    #endregion

    /// <summary>
    /// Metiz va mahkamlash buyumlari (vint, bolt, dyubel) / Метизы и крепёжные изделия (винты, болты, дюбели)
    /// </summary>
    [Display(Name = "Метизы и крепёжные изделия")]
    Fastener = 70,

    /// <summary>
    /// Bir martalik sarf materiallari (skotch, plyonka va h.k.) / Расходные материалы (скотч, плёнка и др.)
    /// </summary>
    [Display(Name = "Расходные материалы")]
    Consumable = 71,

    /// <summary>
    /// Boshqa / Другое
    /// </summary>
    [Display(Name = "Другое")]
    Other = 99,
}
