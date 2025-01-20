WEBGL linki:
https://play.unity.com/en/games/e41649dc-10f8-4104-814c-f1218bc2570a/gorkem-senol



# Oyun Mekanikleri ve Kurallar

## Oyun Başlangıcı
- Oyun başladığında, 7 çift nesne (toplam 14 nesne) yüksekten oyun alanına düşer.

## Oyun Kuralları
- Aynı nesneler alanda bir araya geldiğinde Cisimler patlama animasyonu ile yok olur ve oyuncuya 10 puan kazandırır.
- Eğer alana yerleştirilen ilk cisim ile ikinci cisim farklıysa:
  - **İkinci cisim eski yerine fırlatılır.**

## Oyun Alanı Kontrolü
- Cisimlerin oyun alanından çıkmasına izin verilmez.
- Eğer bir cisim oyun alanından düşecek gibi olursa:
  - **Cisim tekrar oyun alanına yukarıdan bırakılır.**

## Oyun Döngüsü
- Ekrandaki bütün nesneler eşleştirildiğinde, oyuna yeni nesneler eklenir ve oyun devam eder.
- Yeniden başlat butonuna tıklanırsa oyun yeniden başlar.

## Yetenekler (Skiller)
-2x Skill:
Kullanıldığında oyuncu ilk eşleşmede 20 puan kazanır.
-Büyüt Skill:
Etkinleştirildiğinde, tüm nesneler 2 saniye boyunca 1.5 katına büyür ve daha belirgin hale gelir.
-Nesneleri Topla Skill:
Tüm oyun alanındaki nesneleri dairesel bir şekilde tek bir alana toplar.
-İpucu Skill:
Oyuncuya, oyun alanında bulunan ve eşleşme yapabilecek bir çift nesneyi gösterir.
Skill Kullanımı:
Her skill, oyun alanında bir buton ile etkinleştirilebilir.
Skiller 5 saniye bekleme sürelerine sahiptir.

