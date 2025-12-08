import { BasicModelClass } from '../shared/models/basic-model';

export class UserDetails extends BasicModelClass {
  login: string;
  name: string;
  email: string;
  surname: string;
  lastSeen: Date;
  registered: Date;
  language: string;
  folderName: string;
}
