# دفتر الحسابات - تطبيق أندرويد محاسبي

تطبيق أندرويد محاسبي لإدارة الديون والمصاريف والعملاء والموردين، مبني بـ C# و .NET MAUI لـ Visual Studio 2022.

## المميزات

- **إدارة العملاء**: إضافة وتعديل وحذف العملاء مع تتبع أرصدتهم
- **إدارة الموردين**: إدارة كاملة للموردين وحساباتهم
- **المعاملات المالية**: تسجيل حركات "له" (دائن) و "عليه" (مدين)
- **المصاريف**: تتبع المصاريف العامة مع تصنيفات
- **التقارير**: ملخص مالي شامل يعرض المستحقات والالتزامات والمصاريف
- **دعم RTL**: واجهة عربية كاملة من اليمين لليسار
- **تعدد العملات**: ريال يمني، ريال سعودي، دولار أمريكي
- **تخزين محلي**: قاعدة بيانات SQLite محلية على الجهاز

## المتطلبات

- Visual Studio 2022 (الإصدار 17.3 أو أحدث)
- .NET 8.0 SDK
- .NET MAUI workload
- Android SDK (API 21+)

## كيفية التشغيل

1. افتح `AccountingApp.sln` في Visual Studio 2022
2. تأكد من تثبيت .NET MAUI workload:
   ```
   dotnet workload install maui
   ```
3. حدد Android emulator أو جهاز متصل
4. اضغط F5 للتشغيل

## هيكل المشروع

```
AccountingApp/
├── Models/          # نماذج البيانات (Client, Supplier, Transaction, Expense)
├── Services/        # خدمة قاعدة البيانات SQLite
├── ViewModels/      # MVVM ViewModels
├── Views/           # صفحات XAML
└── Platforms/       # إعدادات خاصة بالمنصة (Android)
```

## الحزم المستخدمة

- **sqlite-net-pcl**: قاعدة البيانات المحلية
- **SQLitePCLRaw.bundle_green**: محرك SQLite
- **CommunityToolkit.Mvvm**: أدوات MVVM (ObservableObject, RelayCommand)
