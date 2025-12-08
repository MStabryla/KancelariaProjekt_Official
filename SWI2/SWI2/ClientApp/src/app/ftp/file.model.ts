import { BasicModelClass } from "../shared/models/basic-model";

export class FileModel extends BasicModelClass {
  constructor(rawData: unknown) {
    super(rawData);
    this.path = rawData["fullName"] ?? rawData["path"];
    this.type = rawData["type"] === 1 ? "dir" : rawData["type"] === 0 ? "file" : "link";
  }
  type: string;
  name: string;
  path: string;
  created = new Date();
  modified = new Date();
  size: number;
  children: FileModel[]
  dirOpen: boolean;
}
