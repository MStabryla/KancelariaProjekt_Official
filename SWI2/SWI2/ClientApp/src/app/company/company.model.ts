import { BasicModelClass } from '../shared/models/basic-model';

export class Company extends BasicModelClass {
  id: number;
  name: string;
  country: string;
  city: string;
  postoffice: string;
  postalcode: string;
  street: string;
  housenumber: number;
  apartamentNumber: string;
  nip: string;
  creationPlace: string;
  defaultWNAAccount: string;
  defaultMAAccount: string;
  defaultMAVatAccount: string;

}
