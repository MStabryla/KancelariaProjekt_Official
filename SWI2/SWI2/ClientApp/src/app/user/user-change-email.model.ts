import { BasicModelClass } from "../shared/models/basic-model";

export class UserChangeEmail extends BasicModelClass {
  password: string;
  email: string;
  url: string;
}
