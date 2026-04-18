import { Injectable } from '@angular/core';
import { BehaviorSubject } from 'rxjs';

export type AppLang = 'en' | 'hi' | 'gu' | 'ta';

@Injectable({
  providedIn: 'root'
})
export class I18nService {
  private readonly lang$ = new BehaviorSubject<AppLang>('en');
  readonly currentLang$ = this.lang$.asObservable();

  private readonly dict: Record<AppLang, Record<string, string>> = {
    en: {
      dashboard: 'Dashboard',
      inventory: 'Inventory',
      masters: 'Masters',
      purchase: 'Purchase',
      sales: 'Sales',
      gst: 'GST',
      accounting: 'Accounting',
      warehouse: 'Warehouse',
      manufacturing: 'Manufacturing',
      reports: 'Reports',
      users: 'Users',
      mobile: 'Mobile',
      pos: 'POS Billing',
      shareWhatsapp: 'Share via WhatsApp',
      transportDetails: 'Transport Details',
      payUpi: 'Pay via UPI'
    },
    hi: {
      dashboard: 'डैशबोर्ड',
      inventory: 'इन्वेंटरी',
      masters: 'मास्टर्स',
      purchase: 'खरीद',
      sales: 'बिक्री',
      gst: 'जीएसटी',
      accounting: 'अकाउंटिंग',
      warehouse: 'वेयरहाउस',
      manufacturing: 'मैन्युफैक्चरिंग',
      reports: 'रिपोर्ट्स',
      users: 'यूज़र्स',
      mobile: 'मोबाइल',
      pos: 'पीओएस बिलिंग',
      shareWhatsapp: 'व्हाट्सएप पर शेयर करें',
      transportDetails: 'परिवहन विवरण',
      payUpi: 'UPI द्वारा भुगतान करें'
    },
    gu: {
      dashboard: 'ડેશબોર્ડ',
      inventory: 'ઇન્વેન્ટરી',
      masters: 'માસ્ટર્સ',
      purchase: 'ખરીદી',
      sales: 'વેચાણ',
      gst: 'જીએસટી',
      accounting: 'એકાઉન્ટિંગ',
      warehouse: 'વેરહાઉસ',
      manufacturing: 'મેન્યુફેક્ચરિંગ',
      reports: 'રિપોર્ટ્સ',
      users: 'યૂઝર્સ',
      mobile: 'મોબાઇલ',
      pos: 'પોસ બિલિંગ',
      shareWhatsapp: 'WhatsApp દ્વારા શેર કરો',
      transportDetails: 'ટ્રાન્સપોર્ટની વિગતો',
      payUpi: 'UPI દ્વારા ચૂકવો'
    },
    ta: {
      dashboard: 'டாஷ்போர்டு',
      inventory: 'சரக்கு',
      masters: 'மாஸ்டர்கள்',
      purchase: 'கொள்முதல்',
      sales: 'விற்பனை',
      gst: 'ஜிஎஸ்டி',
      accounting: 'கணக்கியல்',
      warehouse: 'கிடங்கு',
      manufacturing: 'உற்பத்தி',
      reports: 'அறிக்கைகள்',
      users: 'பயனர்கள்',
      mobile: 'மொபைல்',
      pos: 'POS பில்லிங்',
      shareWhatsapp: 'வாட்ஸ்அப் மூலம் பகிரவும்',
      transportDetails: 'போக்குவரத்து விவரங்கள்',
      payUpi: 'UPI மூலம் செலுத்தவும்'
    }
  };

  setLang(lang: AppLang): void {
    this.lang$.next(lang);
    localStorage.setItem('app_lang', lang);
  }

  init(): void {
    this.lang$.next('en');
    localStorage.setItem('app_lang', 'en');
  }

  current(): AppLang {
    return this.lang$.value;
  }

  t(key: string): string {
    return this.dict[this.lang$.value][key] ?? key;
  }
}
