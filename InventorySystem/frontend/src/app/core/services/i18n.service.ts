import { Injectable } from '@angular/core';
import { BehaviorSubject } from 'rxjs';

export type AppLang = 'en' | 'hi' | 'gu';

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
      pos: 'POS Billing'
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
      pos: 'पीओएस बिलिंग'
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
      pos: 'પોસ બિલિંગ'
    }
  };

  setLang(lang: AppLang): void {
    this.lang$.next(lang);
    localStorage.setItem('app_lang', lang);
  }

  init(): void {
    const saved = localStorage.getItem('app_lang') as AppLang | null;
    if (saved && this.dict[saved]) {
      this.lang$.next(saved);
    }
  }

  current(): AppLang {
    return this.lang$.value;
  }

  t(key: string): string {
    return this.dict[this.lang$.value][key] ?? key;
  }
}
