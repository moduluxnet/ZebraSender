# ZebraSender

## English

### Why this project exists
Yurt İçi Kargo used to provide a small Java-based bridge application for sending label files directly to Zebra printers. That application no longer works reliably. To keep label printing smooth and simple, we created this lightweight .NET 8+ console application as a replacement.

### What `.ykss` files are
The `.ykss` files provided by Yurt İçi Kargo are plain text files containing **ZPL (Zebra Programming Language)** commands. They are nothing more than ASCII instructions for a Zebra printer, wrapped in a custom file extension.  

This application can read `.ykss` files (or any other ZPL-compatible text file, e.g. `.txt`, `.zpl`) and send them directly to the configured Zebra printer.  

- Input: a file path to `.ykss` (or `.zpl`)  
- Config: `printer_config.json` in the application folder, containing the target printer name  
- Output: the raw label is printed immediately on the selected printer  

### Default program association
If you set **ZebraSender.exe** as the default program for `.ykss` or `.zpl` files in Windows, you can simply double-click any label file and it will be printed directly to the configured printer without needing to open a terminal.  

### Development notes
The first version of this app was written quickly but contained bugs (such as missing print job handling, BOM issues, and incorrect use of `WritePrinter`). With only a few prompts and refinements, the corrected version was produced, including proper `StartDocPrinter` / `EndDocPrinter` usage and clean error handling.  

### How to use
1. Place your `.ykss` file (or `.zpl`) somewhere accessible.  
2. Create `printer_config.json` in the same folder as the executable:

   ```json
   {
     "printerName": "ZDesigner ZD420-203dpi ZPL"
   }
   ```

   (Make sure the name matches exactly what Windows shows in *Devices and Printers*.)  

3. Run the application:

   ```bash
   ZebraSender.exe "C:\labels\sample.ykss"
   ```

   Or just double-click the `.ykss` / `.zpl` file if you have set ZebraSender as the default program.  

### Sample `.ykss` / ZPL file
You can create a test file named `test.ykss` with the following content:

```
^XA
^FO50,50^ADN,36,20^FDHello Zebra!^FS
^XZ
```

When sent to a Zebra printer, this will print a simple label with the text **Hello Zebra!**.

---

## Türkçe

### Neden bu proje var
Yurt İçi Kargo, etiket dosyalarını Zebra yazıcılara gönderebilmek için küçük bir Java köprü uygulaması sunuyordu. Ancak bu uygulama artık sorunsuz çalışmıyor. Etiket baskısının kolayca devam edebilmesi için .NET 8+ tabanlı bu basit konsol uygulaması geliştirildi.  

### `.ykss` dosyası nedir
Yurt İçi Kargo’nun verdiği `.ykss` dosyaları aslında **ZPL (Zebra Programming Language)** komutları içeren düz metin dosyalarıdır. Yalnızca özel bir uzantı kullanılmıştır.  

Bu uygulama `.ykss` dosyalarını (veya uyumlu diğer ZPL metin dosyalarını, örn. `.txt`, `.zpl`) okuyarak doğrudan belirtilen Zebra yazıcıya gönderir.  

- Girdi: `.ykss` veya `.zpl` dosya yolu  
- Ayar: Uygulama klasöründeki `printer_config.json`, yazıcı adını içerir  
- Çıktı: Etiket seçilen yazıcıya anında basılır  

### Varsayılan program atama
Windows’ta `.ykss` veya `.zpl` dosya uzantısını **ZebraSender.exe** ile ilişkilendirirseniz, dosyaya çift tıklamanız yeterlidir. Program dosyayı otomatik olarak okuyacak ve yazıcıya gönderecektir. Terminal açmanıza gerek kalmaz.  

### Geliştirme notları
Uygulamanın ilk hali hızlıca yazılmış ancak hatalıydı (örneğin yazdırma işinin doğru başlatılmaması, BOM sorunları ve `WritePrinter` fonksiyonunun eksik kullanımı). Birkaç kısa yönlendirme ile hatalar düzeltilmiş, `StartDocPrinter` / `EndDocPrinter` kullanımı eklenmiş ve sağlam hata yakalama sağlanmıştır.  

### Nasıl kullanılır
1. `.ykss` (veya `.zpl`) dosyanızı hazır edin.  
2. Çalıştırılabilir dosyanın yanına `printer_config.json` oluşturun:

   ```json
   {
     "printerName": "ZDesigner ZD420-203dpi ZPL"
   }
   ```

   (Yazıcı adı Windows’un *Aygıtlar ve Yazıcılar* ekranında görünen ad ile birebir aynı olmalıdır.)  

3. Programı çalıştırın:

   ```bash
   ZebraSender.exe "C:\etiketler\ornek.ykss"
   ```

   Ya da `.ykss` / `.zpl` dosyasına çift tıklayın; eğer ZebraSender’ı bu uzantılar için varsayılan program olarak ayarladıysanız, etiket doğrudan yazdırılacaktır.  

### Örnek `.ykss` / ZPL dosyası
`test.ykss` adında bir test dosyası oluşturup şu içeriği koyabilirsiniz:

```
^XA
^FO50,50^ADN,36,20^FDMerhaba Zebra!^FS
^XZ
```

Bu dosya yazıcıya gönderildiğinde üzerinde **Merhaba Zebra!** yazan basit bir etiket çıkaracaktır.
