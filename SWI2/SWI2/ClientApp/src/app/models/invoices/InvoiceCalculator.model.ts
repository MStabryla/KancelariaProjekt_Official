export interface InvoiceCalculator {
  vat: number;
  nettoWorth: number;
  bruttoWorth: number;
  paymentCurrency: string;
  paymentValue: number;
  paymentStatus: number;
}
