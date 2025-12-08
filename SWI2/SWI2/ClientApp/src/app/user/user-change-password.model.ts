import { BasicModelClass } from "../shared/models/basic-model";

export class UserChangePassword extends BasicModelClass {
  previousPassword: string;
  password: string;
  confirmPassword: string;
}
