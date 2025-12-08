import { BasicModelClass } from "../shared/models/basic-model";

export class LetterModel extends BasicModelClass {
  id: number;
  outLetter: boolean;
  notes: string;
  registeredNumbr: string;
  created: Date;
  isPaid: boolean;
  isRegistered: boolean;
  isEmail: boolean;
  isNormal: boolean;
  isCourier: boolean;
  hasLetterFile: boolean;
  withConfirm: boolean;
  letterFileType: string;
  letterRecipientId: number;
}
