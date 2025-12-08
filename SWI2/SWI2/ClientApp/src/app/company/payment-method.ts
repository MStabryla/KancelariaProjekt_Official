import { BasicModelClass } from '../shared/models/basic-model';

export class PaymentMethod extends BasicModelClass {
  id: number;
  name: string;
  accountNumber: string;
  currency: string;
}
