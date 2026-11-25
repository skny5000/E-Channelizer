# GitHub Actions - بناء تلقائي في السحابة (مجاني)

هذا الدليل يشرح كيف تحصل على ملف `.exe` جاهز **دون تثبيت أي شيء على جهازك** - البناء يتم على سيرفرات GitHub مجاناً.

---

## 🎯 ما ستحصل عليه

| الميزة | الوصف |
|----------|-------|
| ✅ بناء أوتوماتيكي | عند كل `git push` |
| ✅ ملفات `.exe` جاهزة | حملها من صفحة Releases |
| ✅ مثبت Inno Setup | مُنشأ تلقائياً عند الإصدار |
| ✅ مجاني 100% | 2000 دقيقة/شهر مجاناً |
| ✅ لا تحتاج .NET على جهازك | البناء في السحابة |

---

## 📋 الخطوات (5 دقائق فقط)

### 1. أنشئ مستودع (Repository) على GitHub

1. اذهب إلى: https://github.com/new
2. **Repository name**: `E-Channelizer`
3. **Description**: `DVB Channel Editor for Satellite, Cable and Terrestrial`
4. اختر **Public** (مجاني) أو **Private**
5. **لا تحدد** أي ملفات (.gitignore, README, License) - سنرفعها نحن
6. اضغط **Create repository**

### 2. ثبت GitHub CLI (اختياري لكن أسهل)

**الطريقة السهلة - winget:**
```cmd
winget install GitHub.cli
```

**أو حمل من:** https://cli.github.com/

### 3. شغل سكريبت الإعداد الأوتوماتيكي

افتح **PowerShell كمسؤول** في مجلد المشروع:

```powershell
cd C:\E-Channelizer-Clone
.\setup-github.ps1 -GitHubUsername "اسم_مستخدمك_على_GitHub"
```

> **مثال:** `.\setup-github.ps1 -GitHubUsername "ahmed"`

السكريبت سيقوم بـ:
- ✅ تهيئة Git
- ✅ إنشاء `.gitignore`
- ✅ عمل commit أولي
- ✅ ربط المستودع البعيد
- ✅ رفع الكود لـ GitHub

### 4. فعل GitHub Actions

1. اذهب إلى: `https://github.com/اسم_مستخدمك/E-Channelizer/actions`
2. إذا ظهرت رسالة **"Workflows aren't being run on this repository"** اضغط **Enable Actions**
3. اختر **"Allow all actions and reusable workflows"**

### 5. أطلق أول بناء (بإنشاء إصدار)

**الطريقة السهلة - من واجهة GitHub:**
1. اذهب إلى: `https://github.com/اسم_مستخدمك/E-Channelizer/releases`
2. اضغط **Create a new release**
3. **Tag version**: `v1.0.0`
4. **Release title**: `E-Channelizer v1.0.0`
5. اضغط **Publish release**

**أو من سطر الأوامر:**
```cmd
cd C:\E-Channelizer-Clone
git tag v1.0.0
git push origin v1.0.0
```

---

## ⏳ انتظر البناء (3-5 دقائق)

1. راقب التقدم في: `https://github.com/اسم_مستخدمك/E-Channelizer/actions`
2. ستظهر 3 jobs:
   - **Build & Publish** - يبني الملفات التنفيذية
   - **Build Installer** - ينشئ المثبت (عند tag فقط)
   - **Code Quality** - يفحص الكود

---

## 📥 حمل الملفات الجاهزة

عند اكتمال البناء (أخضر ✅):

1. اذهب إلى: `https://github.com/اسم_مستخدمك/E-Channelizer/releases`
2. ستجد إصدار `v1.0.0` مع الملفات:
   - `E-Channelizer.exe` - **ذاتي (Self-contained، ~85 MB)** ✅ **استخدم هذا**
   - `E-Channelizer-Portable.exe` - **صغير (~20 MB)** يحتاج .NET Runtime
   - `E-Channelizer-Setup-1.0.0.exe` - **مثبت احترافي**

---

## 🔄 للإصدارات المستقبلية

فقط أنشئ tag جديد:

```cmd
git tag v1.0.1
git push origin v1.0.1
```

أو من واجهة GitHub: **Releases → Create a new release → Tag: v1.0.1**

---

## 🛠 استكشاف الأخطاء

### "Workflow failed" - راجع السجل
1. اضغط على الـ job الفاشل في Actions
2. اضغط على الخطوة الحمراء
3. اقرأ الخطأ

### أخطاء شائعة:

| الخطأ | الحل |
|--------|-------|
| `dotnet: command not found` | GitHub Actions يثبت .NET تلقائياً - جرب إعادة التشغيل |
| `ISCC.exe not found` | المثبت يعمل فقط عند **tag** (v1.0.0)، ليس عند كل push |
| `Permission denied` | تأكد أن المستودع Public أو أن لديك صلاحيات |

### إعادة تشغيل يدوي:
1. Go to Actions tab
2. Select workflow "Build E-Channelizer"
3. Click **Run workflow** → **Run workflow**

---

## 📊 مقارنة الطرق

| الطريقة | الجهد | التكلفة | ملفات .exe | مثبـت |
|----------|-------|---------|-----------|-------|
| **GitHub Actions** ⭐ | 5 دقائق مرة واحدة | **مجاني** | ✅ أوتوماتيكي | ✅ أوتوماتيكي |
| بناء محلي | 30+ دقيقة | مجاني | ✅ يدوي | ✅ يدوي |
| Azure DevOps | 15 دقيقة | مجاني (محدود) | ✅ | ✅ |
| AppVeyor | 10 دقيقة | مجاني (Open Source) | ✅ | ✅ |

---

## 🎉 النتيجة النهائية

ستحصل على رابط دائم للملفات:
```
https://github.com/اسم_مستخدمك/E-Channelizer/releases/latest/download/E-Channelizer.exe
```

**شارك هذا الرابط مع أي شخص** - سيعمل على أي Windows 10/11 فوراً!

---

## 💡 نصائح إضافية

### إضافة أيقونة للمستودع:
1. Settings → General → Repository logo
2. ارفع `Resources/icon.ico` أو صورة PNG

### إضافة وصف للمشروع:
1. Settings → General → Description
2. اكتب: `Professional DVB Channel Editor for Satellite, Cable and Terrestrial TV`

### تفعيل Discussions (للدعم):
1. Settings → Features → Discussions ✅

### إضافة Topics للاكتشاف:
1. Settings → General → Topics
2. أضف: `dvb`, `satellite`, `channel-editor`, `wpf`, `dotnet`, `tv`

---

## 📞 تحتاج مساعدة؟

- **GitHub Actions Docs**: https://docs.github.com/en/actions
- **Issues**: افتح issue في المستودع
- **Discussions**: لأسئلة عامة

---

**هذا كل شيء! 🎉** بعد الإعداد مرة واحدة، ستحصل على ملفات `.exe` محدثة تلقائياً مع كل إصدار.