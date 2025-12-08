import { BasicModelClass } from "../shared/models/basic-model";

export class UserModel extends BasicModelClass {
  id: string;
  userName: string;
  userRole: string;
  phoneNumber: string;
  email: string;
  created: Date;
  isActive: boolean;
}
