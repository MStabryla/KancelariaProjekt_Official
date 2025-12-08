import { BasicModelClass } from "../shared/models/basic-model";

export class DocumentModel extends BasicModelClass {
  
  id: number;
  notes: string;
  comment: string;
  isProtocol: boolean;
  outDocument: boolean;
  companyId: number;
  hasDocumentFile: boolean;
  documentTypeId: number;
  documentFileType: string;
  path: string;
  created: Date;
}
